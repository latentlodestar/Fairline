using Fairline.Domain.Ingest;

namespace Fairline.Domain.Tests.Ingest;

public sealed class OddsRecordTests
{
    private static readonly DateTimeOffset Now = new(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid ProviderId = Guid.NewGuid();

    [Fact]
    public void Create_SetsAllProperties()
    {
        var record = OddsRecord.Create(ProviderId, "nfl:dal-vs-phi", "moneyline", "DAL", 2.10m, """{"raw":true}""", Now);

        record.Id.Should().NotBeEmpty();
        record.ProviderId.Should().Be(ProviderId);
        record.EventKey.Should().Be("nfl:dal-vs-phi");
        record.Market.Should().Be("moneyline");
        record.Selection.Should().Be("DAL");
        record.Odds.Should().Be(2.10m);
        record.RawPayload.Should().Be("""{"raw":true}""");
        record.IngestedAt.Should().Be(Now);
    }

    [Fact]
    public void Create_ThrowsWhenOddsNotPositive()
    {
        var act = () => OddsRecord.Create(ProviderId, "event", "market", "sel", 0m, null, Now);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_ThrowsWhenNegativeOdds()
    {
        var act = () => OddsRecord.Create(ProviderId, "event", "market", "sel", -1.5m, null, Now);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ThrowsOnInvalidEventKey(string? eventKey)
    {
        var act = () => OddsRecord.Create(ProviderId, eventKey!, "market", "sel", 1.5m, null, Now);

        act.Should().Throw<ArgumentException>();
    }
}
