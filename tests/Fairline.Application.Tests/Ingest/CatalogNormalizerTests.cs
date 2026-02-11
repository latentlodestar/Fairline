using Fairline.Abstractions.Contracts;
using Fairline.Application.Ingest;

namespace Fairline.Application.Tests.Ingest;

public sealed class CatalogNormalizerTests
{
    private static readonly DateTimeOffset Now = new(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Normalize_ParsesSportsIntoEntries()
    {
        var sports = new List<OddsApiSport>
        {
            new("americanfootball_nfl", "American Football", "NFL", "US Football", true, false),
            new("basketball_nba", "Basketball", "NBA", "US Basketball", true, false),
            new("soccer_epl", "Soccer", "EPL", "English Premier League", true, false),
            new("icehockey_nhl", "Ice Hockey", "NHL", "US Ice Hockey", false, false),
        };

        var result = CatalogNormalizer.Normalize(sports, Now);

        result.Should().HaveCount(4);

        var nfl = result.First(r => r.ProviderSportKey == "americanfootball_nfl");
        nfl.Title.Should().Be("NFL");
        nfl.Group.Should().Be("American Football");
        nfl.Active.Should().BeTrue();
        nfl.NormalizedSport.Should().Be("american_football");
        nfl.NormalizedLeague.Should().Be("nfl");
        nfl.CapturedAtUtc.Should().Be(Now);

        var nba = result.First(r => r.ProviderSportKey == "basketball_nba");
        nba.NormalizedSport.Should().Be("basketball");
        nba.NormalizedLeague.Should().Be("nba");

        var epl = result.First(r => r.ProviderSportKey == "soccer_epl");
        epl.NormalizedSport.Should().Be("soccer");
        epl.NormalizedLeague.Should().Be("epl");

        var nhl = result.First(r => r.ProviderSportKey == "icehockey_nhl");
        nhl.NormalizedSport.Should().Be("ice_hockey");
        nhl.NormalizedLeague.Should().Be("nhl");
        nhl.Active.Should().BeFalse();
    }

    [Fact]
    public void NormalizeSport_ConvertsGroupToSnakeCase()
    {
        CatalogNormalizer.NormalizeSport("American Football").Should().Be("american_football");
        CatalogNormalizer.NormalizeSport("Ice Hockey").Should().Be("ice_hockey");
        CatalogNormalizer.NormalizeSport("Basketball").Should().Be("basketball");
        CatalogNormalizer.NormalizeSport("Mixed Martial Arts").Should().Be("mixed_martial_arts");
    }

    [Fact]
    public void NormalizeLeague_ExtractsSuffixFromKey()
    {
        CatalogNormalizer.NormalizeLeague("americanfootball_nfl", "NFL").Should().Be("nfl");
        CatalogNormalizer.NormalizeLeague("basketball_nba", "NBA").Should().Be("nba");
        CatalogNormalizer.NormalizeLeague("soccer_germany_bundesliga", "Bundesliga - Germany").Should().Be("germany_bundesliga");
    }

    [Fact]
    public void NormalizeLeague_FallsBackToTitle_WhenNoUnderscore()
    {
        CatalogNormalizer.NormalizeLeague("mma", "MMA").Should().Be("mma");
    }

    [Fact]
    public void Normalize_HandlesEmptyList()
    {
        var result = CatalogNormalizer.Normalize([], Now);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Normalize_PreservesOutrightsFlag()
    {
        var sports = new List<OddsApiSport>
        {
            new("golf_pga", "Golf", "PGA Championship", "PGA", true, true),
        };

        var result = CatalogNormalizer.Normalize(sports, Now);
        result.Should().ContainSingle().Which.HasOutrights.Should().BeTrue();
    }
}
