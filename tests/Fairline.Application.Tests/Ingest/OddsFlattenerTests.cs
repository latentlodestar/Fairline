using Fairline.Abstractions.Contracts;
using Fairline.Application.Ingest;

namespace Fairline.Application.Tests.Ingest;

public sealed class OddsFlattenerTests
{
    private static readonly DateTimeOffset Now = new(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid EventId = Guid.NewGuid();

    [Fact]
    public void Flatten_ProducesCorrectSnapshotsFromNestedStructure()
    {
        var bookmakers = new List<OddsApiBookmaker>
        {
            new("draftkings", "DraftKings", "2025-06-01T11:30:00Z", new List<OddsApiMarket>
            {
                new("h2h", null, new List<OddsApiOutcome>
                {
                    new("Team A", 1.91m, null, null),
                    new("Team B", 1.95m, null, null),
                }),
                new("spreads", null, new List<OddsApiOutcome>
                {
                    new("Team A", 1.87m, -3.5m, null),
                    new("Team B", 1.93m, 3.5m, null),
                }),
            }),
            new("pinnacle", "Pinnacle", "2025-06-01T11:28:00Z", new List<OddsApiMarket>
            {
                new("h2h", null, new List<OddsApiOutcome>
                {
                    new("Team A", 1.90m, null, null),
                    new("Team B", 1.96m, null, null),
                }),
            }),
        };

        var result = OddsFlattener.Flatten(EventId, bookmakers, Now);

        // DraftKings h2h (2) + spreads (2) + Pinnacle h2h (2) = 6
        result.Should().HaveCount(6);

        result.Should().AllSatisfy(s =>
        {
            s.EventId.Should().Be(EventId);
            s.CapturedAtUtc.Should().Be(Now);
        });

        var dkH2h = result.Where(s => s.BookmakerKey == "draftkings" && s.MarketKey == "h2h").ToList();
        dkH2h.Should().HaveCount(2);
        dkH2h.Should().Contain(s => s.OutcomeName == "Team A" && s.Price == 1.91m);
        dkH2h.Should().Contain(s => s.OutcomeName == "Team B" && s.Price == 1.95m);

        var dkSpreads = result.Where(s => s.BookmakerKey == "draftkings" && s.MarketKey == "spreads").ToList();
        dkSpreads.Should().HaveCount(2);
        dkSpreads.Should().Contain(s => s.OutcomeName == "Team A" && s.Point == -3.5m);
        dkSpreads.Should().Contain(s => s.OutcomeName == "Team B" && s.Point == 3.5m);
    }

    [Fact]
    public void Flatten_HandlesEmptyBookmakers()
    {
        var result = OddsFlattener.Flatten(EventId, [], Now);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Flatten_HandlesTotalsMarket()
    {
        var bookmakers = new List<OddsApiBookmaker>
        {
            new("draftkings", "DraftKings", "2025-06-01T11:30:00Z", new List<OddsApiMarket>
            {
                new("totals", null, new List<OddsApiOutcome>
                {
                    new("Over", 1.91m, 52.5m, null),
                    new("Under", 1.91m, 52.5m, null),
                }),
            }),
        };

        var result = OddsFlattener.Flatten(EventId, bookmakers, Now);

        result.Should().HaveCount(2);
        result.Should().Contain(s => s.OutcomeName == "Over" && s.Point == 52.5m);
        result.Should().Contain(s => s.OutcomeName == "Under" && s.Point == 52.5m);
    }

    [Fact]
    public void Flatten_ParsesProviderLastUpdate()
    {
        var bookmakers = new List<OddsApiBookmaker>
        {
            new("pinnacle", "Pinnacle", "2025-06-01T11:28:00Z", new List<OddsApiMarket>
            {
                new("h2h", null, new List<OddsApiOutcome>
                {
                    new("Team A", 1.90m, null, null),
                }),
            }),
        };

        var result = OddsFlattener.Flatten(EventId, bookmakers, Now);
        var snapshot = result.Should().ContainSingle().Subject;
        snapshot.ProviderLastUpdate.Should().Be(new DateTimeOffset(2025, 6, 1, 11, 28, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Flatten_HandlesNullLastUpdate()
    {
        var bookmakers = new List<OddsApiBookmaker>
        {
            new("test", "Test", null, new List<OddsApiMarket>
            {
                new("h2h", null, new List<OddsApiOutcome>
                {
                    new("Team A", 2.00m, null, null),
                }),
            }),
        };

        var result = OddsFlattener.Flatten(EventId, bookmakers, Now);
        result.Should().ContainSingle().Which.ProviderLastUpdate.Should().BeNull();
    }
}
