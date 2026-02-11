using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;

namespace Fairline.Application.Dashboard;

public sealed class GetDashboardHandler(IIngestRepository repo)
{
    public async Task<DashboardResponse> HandleAsync(CancellationToken ct = default)
    {
        var snapshots = await repo.GetLatestSnapshotsAsync(ct);

        var kpis = new DashboardKpis(
            EventCount: snapshots.Select(s => s.SportEventId).Distinct().Count(),
            SnapshotCount: snapshots.Count,
            BookCount: snapshots.Select(s => s.BookmakerKey).Distinct().Count(),
            LatestCaptureUtc: snapshots.Count > 0
                ? snapshots.Max(s => s.CapturedAtUtc)
                : null);

        var edges = snapshots
            .GroupBy(s => (s.SportEventId, s.MarketKey, s.OutcomeName))
            .Where(g => g.Count() >= 2)
            .SelectMany(ComputeEdges)
            .OrderByDescending(e => Math.Abs(e.EdgePct))
            .ToList();

        return new DashboardResponse(kpis, edges);
    }

    private static IEnumerable<EdgeRow> ComputeEdges(
        IGrouping<(Guid SportEventId, string MarketKey, string OutcomeName),
            SnapshotWithEvent> group)
    {
        var items = group.ToList();
        var first = items[0];
        var isPointMarket = first.MarketKey is "spreads" or "totals";

        if (isPointMarket)
        {
            var points = items.Where(i => i.Point.HasValue).Select(i => i.Point!.Value).ToList();
            if (points.Count < 2) yield break;

            var fairPoint = Median(points);

            foreach (var item in items)
            {
                if (!item.Point.HasValue) continue;
                var edgePct = item.Point.Value - fairPoint;
                var signal = Classify(edgePct);

                yield return new EdgeRow(
                    first.SportEventId, first.HomeTeam, first.AwayTeam,
                    first.SportKey, first.SportTitle,
                    first.MarketKey, first.OutcomeName,
                    item.BookmakerKey, item.BookmakerTitle,
                    fairPoint, item.Point.Value, Math.Round(edgePct, 2), signal);
            }
        }
        else
        {
            // h2h: work with implied probabilities
            var probs = items.Select(i => AmericanToImpliedProb(i.Price)).ToList();
            if (probs.Count < 2) yield break;

            var fairProb = Median(probs);
            var fairLine = ImpliedProbToAmerican(fairProb);

            foreach (var item in items)
            {
                var bookProb = AmericanToImpliedProb(item.Price);
                var edgePct = (fairProb - bookProb) * 100m;
                var signal = Classify(edgePct);

                yield return new EdgeRow(
                    first.SportEventId, first.HomeTeam, first.AwayTeam,
                    first.SportKey, first.SportTitle,
                    first.MarketKey, first.OutcomeName,
                    item.BookmakerKey, item.BookmakerTitle,
                    Math.Round(fairLine, 0), item.Price, Math.Round(edgePct, 2), signal);
            }
        }
    }

    private static string Classify(decimal edgePct)
    {
        var abs = Math.Abs(edgePct);
        if (abs >= 2m && edgePct > 0) return "value";
        if (abs >= 1m && edgePct < 0) return "tax";
        return "fair";
    }

    private static decimal Median(List<decimal> values)
    {
        values.Sort();
        var mid = values.Count / 2;
        return values.Count % 2 == 0
            ? (values[mid - 1] + values[mid]) / 2m
            : values[mid];
    }

    private static decimal AmericanToImpliedProb(decimal american)
    {
        if (american >= 100m)
            return 100m / (american + 100m);
        // negative odds
        return -american / (-american + 100m);
    }

    private static decimal ImpliedProbToAmerican(decimal prob)
    {
        if (prob <= 0m || prob >= 1m) return 0m;
        if (prob >= 0.5m)
            return -(prob / (1m - prob)) * 100m;
        return ((1m - prob) / prob) * 100m;
    }
}
