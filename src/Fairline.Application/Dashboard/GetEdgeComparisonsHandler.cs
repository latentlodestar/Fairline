using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;

namespace Fairline.Application.Dashboard;

public sealed class GetEdgeComparisonsHandler(IIngestRepository repo)
{
    private const decimal LineMismatchTolerance = 1.0m;

    public async Task<EdgeComparisonsResponse> HandleAsync(
        string baselineBook = "pinnacle",
        string targetBook = "draftkings",
        bool showIncomplete = false,
        CancellationToken ct = default)
    {
        var snapshots = await repo.GetLatestSnapshotsAsync(ct);

        var kpis = new DashboardKpis(
            EventCount: snapshots.Select(s => s.SportEventId).Distinct().Count(),
            SnapshotCount: snapshots.Count,
            BookCount: snapshots.Select(s => s.BookmakerKey).Distinct().Count(),
            LatestCaptureUtc: snapshots.Count > 0
                ? snapshots.Max(s => s.CapturedAtUtc)
                : null);

        var groups = snapshots
            .GroupBy(s => (s.SportEventId, s.MarketKey, s.OutcomeName));

        var comparisons = new List<EdgeComparisonRow>();

        foreach (var group in groups)
        {
            var items = group.ToList();
            var row = BuildComparison(items, baselineBook, targetBook);
            if (row is null) continue;
            if (!showIncomplete && row.TargetPrice is null) continue;
            comparisons.Add(row);
        }

        comparisons.Sort((a, b) =>
        {
            var absA = a.EdgePct.HasValue ? Math.Abs(a.EdgePct.Value) : -1m;
            var absB = b.EdgePct.HasValue ? Math.Abs(b.EdgePct.Value) : -1m;
            return absB.CompareTo(absA);
        });

        return new EdgeComparisonsResponse(kpis, comparisons);
    }

    internal static EdgeComparisonRow? BuildComparison(
        List<SnapshotWithEvent> items,
        string baselineBook,
        string targetBook)
    {
        if (items.Count == 0) return null;

        var first = items[0];

        var baseline = items.FirstOrDefault(i =>
            i.BookmakerKey.Equals(baselineBook, StringComparison.OrdinalIgnoreCase));
        var target = items.FirstOrDefault(i =>
            i.BookmakerKey.Equals(targetBook, StringComparison.OrdinalIgnoreCase));

        // If no baseline, try to pick the lowest-margin book (by median approach)
        if (baseline is null && items.Count >= 2)
        {
            baseline = PickFallbackBaseline(items, targetBook);
        }

        if (target is null && baseline is null) return null;

        var lastUpdated = items.Max(i => i.CapturedAtUtc);
        var isPointMarket = first.MarketKey is "spreads" or "totals";

        var baselineDec = baseline is not null ? AmericanToDecimal(baseline.Price) : (decimal?)null;
        var targetDec = target is not null ? AmericanToDecimal(target.Price) : (decimal?)null;

        // Check line mismatch for point markets
        if (isPointMarket && baseline is not null && target is not null)
        {
            if (baseline.Point.HasValue && target.Point.HasValue)
            {
                var diff = Math.Abs(baseline.Point.Value - target.Point.Value);
                if (diff > LineMismatchTolerance)
                {
                    return new EdgeComparisonRow(
                        first.SportEventId, first.HomeTeam, first.AwayTeam,
                        first.SportKey, first.SportTitle,
                        first.MarketKey, first.OutcomeName,
                        baseline.Price, baseline.Point, baselineDec, baseline.BookmakerKey,
                        target.Price, target.Point, targetDec, target.BookmakerKey,
                        EdgePct: null,
                        Signal: "line_mismatch",
                        lastUpdated);
                }
            }
        }

        decimal? edgePct = null;
        string signal;

        if (baseline is null)
        {
            signal = "no_baseline";
        }
        else if (target is null)
        {
            signal = "no_target";
        }
        else
        {
            edgePct = isPointMarket
                ? ComputePointEdge(baseline, target)
                : ComputeMoneylineEdge(baseline, target);

            signal = Classify(edgePct);
        }

        return new EdgeComparisonRow(
            first.SportEventId, first.HomeTeam, first.AwayTeam,
            first.SportKey, first.SportTitle,
            first.MarketKey, first.OutcomeName,
            baseline?.Price, baseline?.Point, baselineDec, baseline?.BookmakerKey,
            target?.Price, target?.Point, targetDec, target?.BookmakerKey ?? targetBook,
            edgePct,
            signal,
            lastUpdated);
    }

    private static SnapshotWithEvent? PickFallbackBaseline(
        List<SnapshotWithEvent> items, string targetBook)
    {
        var candidates = items
            .Where(i => !i.BookmakerKey.Equals(targetBook, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (candidates.Count == 0) return null;

        var first = candidates[0];
        if (first.MarketKey is "spreads" or "totals")
            return candidates[0];

        // For h2h, pick the book offering the best decimal odds (sharpest line)
        return candidates
            .OrderByDescending(c => AmericanToDecimal(c.Price))
            .First();
    }

    internal static decimal? ComputePointEdge(SnapshotWithEvent baseline, SnapshotWithEvent target)
    {
        if (!baseline.Point.HasValue || !target.Point.HasValue) return null;
        return Math.Round(target.Point.Value - baseline.Point.Value, 2);
    }

    /// <summary>
    /// Canonical edge formula using decimal odds:
    ///   edgePct = (targetDec / baselineDec - 1) * 100
    /// </summary>
    internal static decimal? ComputeMoneylineEdge(SnapshotWithEvent baseline, SnapshotWithEvent target)
    {
        var baseDec = AmericanToDecimal(baseline.Price);
        var targetDec = AmericanToDecimal(target.Price);

        if (baseDec <= 0m) return null;

        return Math.Round((targetDec / baseDec - 1m) * 100m, 2);
    }

    internal static string Classify(decimal? edgePct)
    {
        if (!edgePct.HasValue) return "no_baseline";
        var val = edgePct.Value;
        if (val >= 1m) return "value";
        if (val <= -1m) return "tax";
        return "fair";
    }

    /// <summary>
    /// American -> Decimal odds conversion.
    ///   am > 0:  dec = 1 + (am / 100)
    ///   am &lt; 0:  dec = 1 + (100 / |am|)
    /// </summary>
    internal static decimal AmericanToDecimal(decimal american)
    {
        if (american > 0m)
            return 1m + american / 100m;
        if (american < 0m)
            return 1m + 100m / Math.Abs(american);
        // Edge case: am == 0 → return 1 (even money with no profit, degenerate)
        return 1m;
    }
}
