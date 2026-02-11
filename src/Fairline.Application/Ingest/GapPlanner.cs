using Fairline.Abstractions.Contracts;

namespace Fairline.Application.Ingest;

public static class GapPlanner
{
    public static IReadOnlyList<string> DetermineLeaguesToRefresh(
        IReadOnlyList<TrackedLeagueState> leagues,
        DateTimeOffset now)
    {
        var result = new List<string>();

        foreach (var league in leagues)
        {
            if (!league.Enabled || !league.Active) continue;

            var freshness = GetRequiredFreshness(league.EarliestEventCommenceTime, now);

            if (league.LatestSnapshotCapturedAtUtc is null ||
                now - league.LatestSnapshotCapturedAtUtc.Value > freshness)
            {
                result.Add(league.ProviderSportKey);
            }
        }

        return result;
    }

    public static TimeSpan GetRequiredFreshness(DateTimeOffset? earliestCommence, DateTimeOffset now)
    {
        if (earliestCommence is null)
            return TimeSpan.FromHours(6);

        var hoursUntilStart = (earliestCommence.Value - now).TotalHours;
        return hoursUntilStart switch
        {
            <= 24 => TimeSpan.FromMinutes(10),
            <= 72 => TimeSpan.FromMinutes(60),
            _ => TimeSpan.FromHours(6)
        };
    }
}
