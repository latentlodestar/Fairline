using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;
using Fairline.Application.Dashboard;

namespace Fairline.Application.Tests.Dashboard;

public sealed class GetEdgeComparisonsHandlerTests
{
    private static readonly DateTimeOffset Now = new(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid EventId = Guid.NewGuid();

    private readonly IIngestRepository _repo = Substitute.For<IIngestRepository>();
    private readonly GetEdgeComparisonsHandler _handler;

    public GetEdgeComparisonsHandlerTests()
    {
        _repo.GetSportCatalogAsync(Arg.Any<CancellationToken>())
            .Returns(new List<SportCatalogEntry>());
        _handler = new GetEdgeComparisonsHandler(_repo);
    }

    private static SnapshotWithEvent Snap(
        string bookKey, string bookTitle, string market, string outcome,
        decimal price, decimal? point = null, Guid? eventId = null) =>
        new(eventId ?? EventId, "HomeTeam", "AwayTeam",
            "soccer_epl", "EPL", Now.AddDays(1),
            bookKey, bookTitle, market, outcome,
            price, point, Now);

    // ---------------------------------------------------------------
    // AmericanToDecimal
    // ---------------------------------------------------------------

    [Theory]
    [InlineData(44, 1.44)]
    [InlineData(26, 1.26)]
    [InlineData(100, 2.0)]
    [InlineData(180, 2.80)]
    [InlineData(195, 2.95)]
    [InlineData(200, 3.0)]
    [InlineData(25, 1.25)]
    [InlineData(17, 1.17)]
    public void AmericanToDecimal_PositiveOdds(decimal american, decimal expectedDecimal)
    {
        GetEdgeComparisonsHandler.AmericanToDecimal(american)
            .Should().BeApproximately(expectedDecimal, 0.001m);
    }

    [Theory]
    [InlineData(-135, 1.7407)]
    [InlineData(-140, 1.7143)]
    [InlineData(-150, 1.6667)]
    [InlineData(-130, 1.7692)]
    [InlineData(-110, 1.9091)]
    [InlineData(-200, 1.5)]
    public void AmericanToDecimal_NegativeOdds(decimal american, decimal expectedDecimal)
    {
        GetEdgeComparisonsHandler.AmericanToDecimal(american)
            .Should().BeApproximately(expectedDecimal, 0.001m);
    }

    // ---------------------------------------------------------------
    // Canonical edge formula: (dkDec / pinDec - 1) * 100
    // ---------------------------------------------------------------

    [Fact]
    public void ComputeMoneylineEdge_Pin44_DK26_NegativeTwelvePointFive()
    {
        // Pin +44 (dec 1.44) vs DK +26 (dec 1.26)
        // edge = (1.26 / 1.44 - 1) * 100 = -12.50%
        var baseline = Snap("pinnacle", "Pinnacle", "h2h", "Home", 44);
        var target = Snap("draftkings", "DraftKings", "h2h", "Home", 26);

        var edge = GetEdgeComparisonsHandler.ComputeMoneylineEdge(baseline, target);

        edge.Should().NotBeNull();
        edge!.Value.Should().BeApproximately(-12.50m, 0.05m);
    }

    [Fact]
    public void ComputeMoneylineEdge_Pin25_DK17_NegativeSixPointFour()
    {
        // Pin +25 (dec 1.25) vs DK +17 (dec 1.17)
        // edge = (1.17 / 1.25 - 1) * 100 = -6.40%
        var baseline = Snap("pinnacle", "Pinnacle", "h2h", "Home", 25);
        var target = Snap("draftkings", "DraftKings", "h2h", "Home", 17);

        var edge = GetEdgeComparisonsHandler.ComputeMoneylineEdge(baseline, target);

        edge.Should().NotBeNull();
        edge!.Value.Should().BeApproximately(-6.40m, 0.05m);
    }

    [Fact]
    public void ComputeMoneylineEdge_PinMinus135_DKMinus140_NegativeOnePointFive()
    {
        // Pin -135 (dec 1.7407) vs DK -140 (dec 1.7143)
        // edge = (1.7143 / 1.7407 - 1) * 100 ≈ -1.52%
        var baseline = Snap("pinnacle", "Pinnacle", "h2h", "Home", -135);
        var target = Snap("draftkings", "DraftKings", "h2h", "Home", -140);

        var edge = GetEdgeComparisonsHandler.ComputeMoneylineEdge(baseline, target);

        edge.Should().NotBeNull();
        edge!.Value.Should().BeApproximately(-1.52m, 0.05m);
    }

    [Fact]
    public void ComputeMoneylineEdge_Pin180_DK195_PositiveFivePointThree()
    {
        // Pin +180 (dec 2.80) vs DK +195 (dec 2.95)
        // edge = (2.95 / 2.80 - 1) * 100 ≈ +5.36%
        var baseline = Snap("pinnacle", "Pinnacle", "h2h", "Home", 180);
        var target = Snap("draftkings", "DraftKings", "h2h", "Home", 195);

        var edge = GetEdgeComparisonsHandler.ComputeMoneylineEdge(baseline, target);

        edge.Should().NotBeNull();
        edge!.Value.Should().BeApproximately(5.36m, 0.05m);
    }

    // ---------------------------------------------------------------
    // SANITY CHECK: Pin +44 / DK +26 → ~ -12.5%, signal = Tax
    // ---------------------------------------------------------------

    [Fact]
    public async Task SanityCheck_Pin44_DK26_ProducesTaxAtNegativeTwelvePercent()
    {
        var snapshots = new List<SnapshotWithEvent>
        {
            Snap("pinnacle", "Pinnacle", "h2h", "Home", 44),
            Snap("draftkings", "DraftKings", "h2h", "Home", 26),
        };
        _repo.GetLatestSnapshotsAsync(Arg.Any<CancellationToken>()).Returns(snapshots);

        var result = await _handler.HandleAsync();

        result.Comparisons.Should().HaveCount(1);
        var row = result.Comparisons[0];
        row.EdgePct.Should().NotBeNull();
        row.EdgePct!.Value.Should().BeApproximately(-12.50m, 0.05m);
        row.Signal.Should().Be("tax");
        // Decimal fields populated correctly
        row.BaselineDecimal.Should().BeApproximately(1.44m, 0.001m);
        row.TargetDecimal.Should().BeApproximately(1.26m, 0.001m);
    }

    // ---------------------------------------------------------------
    // Grouping / structural tests
    // ---------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_GroupsTwoBooks_IntoOneRow()
    {
        var snapshots = new List<SnapshotWithEvent>
        {
            Snap("pinnacle", "Pinnacle", "h2h", "Home", -150),
            Snap("draftkings", "DraftKings", "h2h", "Home", -130),
        };
        _repo.GetLatestSnapshotsAsync(Arg.Any<CancellationToken>()).Returns(snapshots);

        var result = await _handler.HandleAsync();

        result.Comparisons.Should().HaveCount(1);
        var row = result.Comparisons[0];
        row.BaselineBook.Should().Be("pinnacle");
        row.TargetBook.Should().Be("draftkings");
        row.BaselinePrice.Should().Be(-150);
        row.TargetPrice.Should().Be(-130);
        // Pin -150 (dec 1.6667), DK -130 (dec 1.7692)
        // edge = (1.7692 / 1.6667 - 1) * 100 ≈ +6.15% → value
        row.EdgePct.Should().NotBeNull();
        row.EdgePct!.Value.Should().BeGreaterThan(0);
        row.Signal.Should().Be("value");
    }

    [Fact]
    public async Task HandleAsync_MissingPinnacle_FallsBackToOtherBook()
    {
        var snapshots = new List<SnapshotWithEvent>
        {
            Snap("betmgm", "BetMGM", "h2h", "Home", -140),
            Snap("draftkings", "DraftKings", "h2h", "Home", -130),
        };
        _repo.GetLatestSnapshotsAsync(Arg.Any<CancellationToken>()).Returns(snapshots);

        var result = await _handler.HandleAsync();

        result.Comparisons.Should().HaveCount(1);
        var row = result.Comparisons[0];
        row.BaselineBook.Should().Be("betmgm");
        row.EdgePct.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_MissingTarget_HiddenByDefault()
    {
        var snapshots = new List<SnapshotWithEvent>
        {
            Snap("pinnacle", "Pinnacle", "h2h", "Home", -150),
            Snap("betmgm", "BetMGM", "h2h", "Home", -140),
        };
        _repo.GetLatestSnapshotsAsync(Arg.Any<CancellationToken>()).Returns(snapshots);

        var result = await _handler.HandleAsync();

        result.Comparisons.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_MissingTarget_ShownWhenShowIncomplete()
    {
        var snapshots = new List<SnapshotWithEvent>
        {
            Snap("pinnacle", "Pinnacle", "h2h", "Home", -150),
            Snap("betmgm", "BetMGM", "h2h", "Home", -140),
        };
        _repo.GetLatestSnapshotsAsync(Arg.Any<CancellationToken>()).Returns(snapshots);

        var result = await _handler.HandleAsync(showIncomplete: true);

        result.Comparisons.Should().HaveCount(1);
        result.Comparisons[0].Signal.Should().Be("no_target");
    }

    [Fact]
    public async Task HandleAsync_LineMismatch_FlaggedCorrectly()
    {
        var snapshots = new List<SnapshotWithEvent>
        {
            Snap("pinnacle", "Pinnacle", "spreads", "Home", -110, -2.5m),
            Snap("draftkings", "DraftKings", "spreads", "Home", -105, -5.0m),
        };
        _repo.GetLatestSnapshotsAsync(Arg.Any<CancellationToken>()).Returns(snapshots);

        var result = await _handler.HandleAsync();

        result.Comparisons.Should().HaveCount(1);
        var row = result.Comparisons[0];
        row.Signal.Should().Be("line_mismatch");
        row.EdgePct.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_SpreadsWithinTolerance_ComputesEdge()
    {
        var snapshots = new List<SnapshotWithEvent>
        {
            Snap("pinnacle", "Pinnacle", "spreads", "Home", -110, -3.0m),
            Snap("draftkings", "DraftKings", "spreads", "Home", -105, -2.5m),
        };
        _repo.GetLatestSnapshotsAsync(Arg.Any<CancellationToken>()).Returns(snapshots);

        var result = await _handler.HandleAsync();

        result.Comparisons.Should().HaveCount(1);
        var row = result.Comparisons[0];
        row.EdgePct.Should().Be(0.5m); // -2.5 - (-3.0) = 0.5
        row.Signal.Should().Be("fair");
    }

    [Fact]
    public async Task HandleAsync_EmptySnapshots_ReturnsEmptyResult()
    {
        _repo.GetLatestSnapshotsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<SnapshotWithEvent>());

        var result = await _handler.HandleAsync();

        result.Comparisons.Should().BeEmpty();
        result.Kpis.EventCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_MultipleSelections_GroupedSeparately()
    {
        var snapshots = new List<SnapshotWithEvent>
        {
            Snap("pinnacle", "Pinnacle", "h2h", "Home", -150),
            Snap("draftkings", "DraftKings", "h2h", "Home", -130),
            Snap("pinnacle", "Pinnacle", "h2h", "Away", 200),
            Snap("draftkings", "DraftKings", "h2h", "Away", 220),
        };
        _repo.GetLatestSnapshotsAsync(Arg.Any<CancellationToken>()).Returns(snapshots);

        var result = await _handler.HandleAsync();

        result.Comparisons.Should().HaveCount(2);
        result.Comparisons.Select(c => c.SelectionKey)
            .Should().BeEquivalentTo(["Home", "Away"]);
    }

    [Fact]
    public void ComputeMoneylineEdge_ValueScenario()
    {
        // Pin -150 (dec 1.6667), DK -130 (dec 1.7692)
        // edge = (1.7692 / 1.6667 - 1) * 100 ≈ +6.15% → value
        var baseline = Snap("pinnacle", "Pinnacle", "h2h", "Home", -150);
        var target = Snap("draftkings", "DraftKings", "h2h", "Home", -130);

        var edge = GetEdgeComparisonsHandler.ComputeMoneylineEdge(baseline, target);

        edge.Should().NotBeNull();
        edge!.Value.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ComputeMoneylineEdge_TaxScenario()
    {
        // Pin +200 (dec 3.0), DK +150 (dec 2.5)
        // edge = (2.5 / 3.0 - 1) * 100 ≈ -16.67% → tax
        var baseline = Snap("pinnacle", "Pinnacle", "h2h", "Home", 200);
        var target = Snap("draftkings", "DraftKings", "h2h", "Home", 150);

        var edge = GetEdgeComparisonsHandler.ComputeMoneylineEdge(baseline, target);

        edge.Should().NotBeNull();
        edge!.Value.Should().BeLessThan(0);
    }

    [Fact]
    public void ComputePointEdge_ReturnsPointDifference()
    {
        var baseline = Snap("pinnacle", "Pinnacle", "spreads", "Home", -110, -3.0m);
        var target = Snap("draftkings", "DraftKings", "spreads", "Home", -110, -2.5m);

        var edge = GetEdgeComparisonsHandler.ComputePointEdge(baseline, target);

        edge.Should().Be(0.5m);
    }

    // ---------------------------------------------------------------
    // Classify thresholds: >=1 → value, <=-1 → tax, else fair
    // ---------------------------------------------------------------

    [Fact]
    public void Classify_ValueThreshold()
    {
        GetEdgeComparisonsHandler.Classify(1.0m).Should().Be("value");
        GetEdgeComparisonsHandler.Classify(5.5m).Should().Be("value");
    }

    [Fact]
    public void Classify_TaxThreshold()
    {
        GetEdgeComparisonsHandler.Classify(-1.0m).Should().Be("tax");
        GetEdgeComparisonsHandler.Classify(-8.0m).Should().Be("tax");
    }

    [Fact]
    public void Classify_FairBand()
    {
        GetEdgeComparisonsHandler.Classify(0.0m).Should().Be("fair");
        GetEdgeComparisonsHandler.Classify(0.99m).Should().Be("fair");
        GetEdgeComparisonsHandler.Classify(-0.99m).Should().Be("fair");
    }

    [Fact]
    public void Classify_Null_ReturnsNoBaseline()
    {
        GetEdgeComparisonsHandler.Classify(null).Should().Be("no_baseline");
    }

    // ---------------------------------------------------------------
    // KPIs / ordering
    // ---------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_KpisAreCorrect()
    {
        var snapshots = new List<SnapshotWithEvent>
        {
            Snap("pinnacle", "Pinnacle", "h2h", "Home", -150),
            Snap("draftkings", "DraftKings", "h2h", "Home", -130),
        };
        _repo.GetLatestSnapshotsAsync(Arg.Any<CancellationToken>()).Returns(snapshots);

        var result = await _handler.HandleAsync();

        result.Kpis.EventCount.Should().Be(1);
        result.Kpis.SnapshotCount.Should().Be(2);
        result.Kpis.BookCount.Should().Be(2);
        result.Kpis.LatestCaptureUtc.Should().Be(Now);
    }

    [Fact]
    public async Task HandleAsync_OrdersByAbsoluteEdgeDescending()
    {
        var eventA = Guid.NewGuid();
        var eventB = Guid.NewGuid();
        var snapshots = new List<SnapshotWithEvent>
        {
            // Small edge
            Snap("pinnacle", "Pinnacle", "h2h", "Home", -150, eventId: eventA),
            Snap("draftkings", "DraftKings", "h2h", "Home", -145, eventId: eventA),
            // Larger edge
            Snap("pinnacle", "Pinnacle", "h2h", "Home", -200, eventId: eventB),
            Snap("draftkings", "DraftKings", "h2h", "Home", -130, eventId: eventB),
        };
        _repo.GetLatestSnapshotsAsync(Arg.Any<CancellationToken>()).Returns(snapshots);

        var result = await _handler.HandleAsync();

        result.Comparisons.Should().HaveCount(2);
        Math.Abs(result.Comparisons[0].EdgePct!.Value)
            .Should().BeGreaterThanOrEqualTo(Math.Abs(result.Comparisons[1].EdgePct!.Value));
    }

    [Fact]
    public async Task HandleAsync_SingleBook_NoComparison()
    {
        var snapshots = new List<SnapshotWithEvent>
        {
            Snap("draftkings", "DraftKings", "h2h", "Home", -130),
        };
        _repo.GetLatestSnapshotsAsync(Arg.Any<CancellationToken>()).Returns(snapshots);

        var result = await _handler.HandleAsync();

        result.Comparisons.Should().HaveCount(1);
        result.Comparisons[0].Signal.Should().Be("no_baseline");
    }

    // ---------------------------------------------------------------
    // Decimal fields populated on DTO
    // ---------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_PopulatesDecimalFields()
    {
        var snapshots = new List<SnapshotWithEvent>
        {
            Snap("pinnacle", "Pinnacle", "h2h", "Home", 180),   // dec 2.80
            Snap("draftkings", "DraftKings", "h2h", "Home", 195), // dec 2.95
        };
        _repo.GetLatestSnapshotsAsync(Arg.Any<CancellationToken>()).Returns(snapshots);

        var result = await _handler.HandleAsync();

        var row = result.Comparisons[0];
        row.BaselineDecimal.Should().BeApproximately(2.80m, 0.001m);
        row.TargetDecimal.Should().BeApproximately(2.95m, 0.001m);
    }
}
