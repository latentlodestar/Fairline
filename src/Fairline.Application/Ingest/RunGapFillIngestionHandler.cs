using System.Diagnostics;
using System.Text.Json;
using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;
using Fairline.Domain.Ingest;

namespace Fairline.Application.Ingest;

public sealed class RunGapFillIngestionHandler(
    IOddsApiClient oddsApiClient,
    IIngestRepository repository,
    IIngestEventSink eventSink,
    IClock clock)
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task HandleAsync(Guid runId, RunIngestionRequest request, CancellationToken ct = default)
    {
        var now = clock.UtcNow;
        var totalRequests = 0;
        var totalEvents = 0;
        var totalSnapshots = 0;
        var totalErrors = 0;

        try
        {
            PublishLog(runId, "Info", "Starting gap-fill ingestion");

            var leagueStates = await repository.GetTrackedLeagueStatesAsync(ct);
            var leaguesToRefresh = GapPlanner.DetermineLeaguesToRefresh(leagueStates, now);

            if (leaguesToRefresh.Count == 0)
            {
                PublishLog(runId, "Info", "No leagues need refreshing — all data is fresh");
                await repository.AddLogAsync(runId, IngestLog.Levels.Info, "No leagues need refreshing", now, ct);
                await repository.CompleteRunAsync(runId, 0, 0, 0, 0, clock.UtcNow, ct);
                PublishSummary(runId, 0, 0, 0, 0);
                return;
            }

            PublishLog(runId, "Info", $"Planned {leaguesToRefresh.Count} league refresh(es): {string.Join(", ", leaguesToRefresh)}");
            PublishProgress(runId, 0, leaguesToRefresh.Count, "Starting...");

            var outrightsBySportKey = leagueStates.ToDictionary(l => l.ProviderSportKey, l => l.HasOutrights);
            var regions = request.Books is { Length: > 0 } ? null : request.Regions;

            for (var i = 0; i < leaguesToRefresh.Count; i++)
            {
                if (ct.IsCancellationRequested)
                {
                    var cancelMsg = $"Cancelled by user after {i}/{leaguesToRefresh.Count} leagues: {totalRequests} requests, {totalEvents} events, {totalSnapshots} snapshots";
                    PublishLog(runId, "Warning", cancelMsg);
                    await repository.AddLogAsync(runId, IngestLog.Levels.Warning, cancelMsg, clock.UtcNow, CancellationToken.None);
                    await repository.CancelRunAsync(runId, totalRequests, totalEvents, totalSnapshots, clock.UtcNow, CancellationToken.None);
                    PublishSummary(runId, totalRequests, totalEvents, totalSnapshots, 0);
                    return;
                }

                var sportKey = leaguesToRefresh[i];
                var supportsOutrights = outrightsBySportKey.GetValueOrDefault(sportKey);
                var markets = supportsOutrights
                    ? request.Markets
                    : request.Markets.Where(m => m != "outrights").ToArray();

                var options = new OddsRequestOptions(
                    Markets: markets,
                    Regions: regions,
                    Bookmakers: request.Books);

                var sw = Stopwatch.StartNew();

                try
                {
                    var events = await oddsApiClient.GetOddsAsync(sportKey, options, ct);
                    sw.Stop();
                    totalRequests++;

                    await repository.AddProviderRequestAsync(
                        runId, $"/v4/sports/{sportKey}/odds", 200, sw.ElapsedMilliseconds,
                        null, null, clock.UtcNow, CancellationToken.None);

                    var leagueSnapshots = 0;

                    foreach (var apiEvent in events)
                    {
                        ct.ThrowIfCancellationRequested();

                        if (!DateTimeOffset.TryParse(apiEvent.CommenceTime, out var commenceTime))
                            continue;

                        var eventId = await repository.UpsertEventAsync(
                            apiEvent.Id, apiEvent.SportKey, apiEvent.SportTitle,
                            apiEvent.HomeTeam, apiEvent.AwayTeam, commenceTime,
                            clock.UtcNow, CancellationToken.None);

                        var snapshots = OddsFlattener.Flatten(eventId, apiEvent.Bookmakers, clock.UtcNow);
                        if (snapshots.Count > 0)
                        {
                            await repository.AddOddsSnapshotsAsync(snapshots, CancellationToken.None);
                            leagueSnapshots += snapshots.Count;
                        }

                        totalEvents++;
                    }

                    totalSnapshots += leagueSnapshots;

                    var msg = $"[{sportKey}] {events.Count} events, {leagueSnapshots} snapshots ({sw.ElapsedMilliseconds}ms)";
                    PublishLog(runId, "Info", msg);
                    await repository.AddLogAsync(runId, IngestLog.Levels.Info, msg, clock.UtcNow, CancellationToken.None);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    totalErrors++;
                    totalRequests++;

                    var errMsg = $"[{sportKey}] Error: {ex.Message}";
                    PublishLog(runId, "Error", errMsg);
                    await repository.AddLogAsync(runId, IngestLog.Levels.Error, errMsg, clock.UtcNow, CancellationToken.None);
                    await repository.AddProviderRequestAsync(
                        runId, $"/v4/sports/{sportKey}/odds", null, sw.ElapsedMilliseconds,
                        null, ex.Message, clock.UtcNow, CancellationToken.None);
                }

                PublishProgress(runId, i + 1, leaguesToRefresh.Count, sportKey);
            }

            var summary = $"Completed: {totalRequests} requests, {totalEvents} events, {totalSnapshots} snapshots, {totalErrors} errors";
            await repository.AddLogAsync(runId, IngestLog.Levels.Info, summary, clock.UtcNow, CancellationToken.None);
            await repository.SetRunSummaryAsync(runId, summary, CancellationToken.None);
            await repository.CompleteRunAsync(runId, totalRequests, totalEvents, totalSnapshots, totalErrors, clock.UtcNow, CancellationToken.None);

            PublishLog(runId, "Info", summary);
            PublishSummary(runId, totalRequests, totalEvents, totalSnapshots, totalErrors);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            var cancelMsg = $"Cancelled by user: {totalRequests} requests, {totalEvents} events, {totalSnapshots} snapshots";
            PublishLog(runId, "Warning", cancelMsg);
            await repository.AddLogAsync(runId, IngestLog.Levels.Warning, cancelMsg, clock.UtcNow, CancellationToken.None);
            await repository.CancelRunAsync(runId, totalRequests, totalEvents, totalSnapshots, clock.UtcNow, CancellationToken.None);
            PublishSummary(runId, totalRequests, totalEvents, totalSnapshots, 0);
        }
        catch (Exception ex)
        {
            totalErrors++;
            PublishLog(runId, "Error", $"Fatal error: {ex.Message}");
            await repository.FailRunAsync(runId, ex.Message, clock.UtcNow, CancellationToken.None);
            PublishSummary(runId, totalRequests, totalEvents, totalSnapshots, totalErrors);
        }
    }

    private void PublishLog(Guid runId, string level, string message)
    {
        var payload = JsonSerializer.Serialize(new { level, message, timestamp = clock.UtcNow }, JsonOpts);
        eventSink.Publish(runId, "log", payload);
    }

    private void PublishProgress(Guid runId, int current, int total, string message)
    {
        var payload = JsonSerializer.Serialize(new { current, total, message }, JsonOpts);
        eventSink.Publish(runId, "progress", payload);
    }

    private void PublishSummary(Guid runId, int requestCount, int eventCount, int snapshotCount, int errorCount)
    {
        var payload = JsonSerializer.Serialize(new { requestCount, eventCount, snapshotCount, errorCount }, JsonOpts);
        eventSink.Publish(runId, "summary", payload);
    }
}
