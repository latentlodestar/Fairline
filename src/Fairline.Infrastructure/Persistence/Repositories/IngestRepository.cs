using Microsoft.EntityFrameworkCore;
using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;
using Fairline.Domain.Ingest;

namespace Fairline.Infrastructure.Persistence.Repositories;

public sealed class IngestRepository(IngestDbContext db) : IIngestRepository
{
    public async Task<Guid> CreateRunAsync(string runType, DateTimeOffset startedAtUtc, CancellationToken ct)
    {
        var run = IngestRun.Start(runType, startedAtUtc);
        db.IngestRuns.Add(run);
        await db.SaveChangesAsync(ct);
        return run.Id;
    }

    public async Task CompleteRunAsync(Guid runId, int requestCount, int eventCount, int snapshotCount, int errorCount, DateTimeOffset completedAtUtc, CancellationToken ct)
    {
        var run = await db.IngestRuns.FindAsync([runId], ct)
            ?? throw new InvalidOperationException($"IngestRun {runId} not found");
        run.Complete(requestCount, eventCount, snapshotCount, errorCount, completedAtUtc);
        await db.SaveChangesAsync(ct);
    }

    public async Task FailRunAsync(Guid runId, string error, DateTimeOffset now, CancellationToken ct)
    {
        var run = await db.IngestRuns.FindAsync([runId], ct)
            ?? throw new InvalidOperationException($"IngestRun {runId} not found");
        run.Fail(error, now);
        await db.SaveChangesAsync(ct);
    }

    public async Task CancelRunAsync(Guid runId, DateTimeOffset now, CancellationToken ct)
    {
        var run = await db.IngestRuns.FindAsync([runId], ct)
            ?? throw new InvalidOperationException($"IngestRun {runId} not found");
        run.Cancel(now);
        await db.SaveChangesAsync(ct);
    }

    public async Task CancelRunAsync(Guid runId, int requestCount, int eventCount, int snapshotCount, DateTimeOffset now, CancellationToken ct)
    {
        var run = await db.IngestRuns.FindAsync([runId], ct)
            ?? throw new InvalidOperationException($"IngestRun {runId} not found");
        run.Cancel(requestCount, eventCount, snapshotCount, now);
        await db.SaveChangesAsync(ct);
    }

    public async Task SetRunSummaryAsync(Guid runId, string summary, CancellationToken ct)
    {
        var run = await db.IngestRuns.FindAsync([runId], ct)
            ?? throw new InvalidOperationException($"IngestRun {runId} not found");
        run.SetSummary(summary);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddLogAsync(Guid runId, string level, string message, DateTimeOffset createdAtUtc, CancellationToken ct)
    {
        var log = IngestLog.Create(runId, level, message, createdAtUtc);
        db.IngestLogs.Add(log);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddProviderRequestAsync(Guid runId, string url, int? statusCode, long? durationMs, int? quotaUsed, string? errorMessage, DateTimeOffset requestedAtUtc, CancellationToken ct)
    {
        var request = ProviderRequest.Create(runId, url, statusCode, durationMs, quotaUsed, errorMessage, requestedAtUtc);
        db.ProviderRequests.Add(request);
        await db.SaveChangesAsync(ct);
    }

    public async Task SaveCatalogSnapshotAsync(string provider, string rawJson, int sportCount, DateTimeOffset capturedAtUtc, CancellationToken ct)
    {
        var snapshot = ProviderCatalogSnapshot.Create(provider, rawJson, sportCount, capturedAtUtc);
        db.ProviderCatalogSnapshots.Add(snapshot);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpsertSportCatalogAsync(IReadOnlyList<SportCatalogEntry> entries, DateTimeOffset capturedAtUtc, CancellationToken ct)
    {
        var existingKeys = await db.SportCatalogs
            .AsNoTracking()
            .Select(s => s.ProviderSportKey)
            .ToListAsync(ct);
        var existingSet = existingKeys.ToHashSet();

        foreach (var entry in entries)
        {
            if (existingSet.Contains(entry.ProviderSportKey))
            {
                var existing = await db.SportCatalogs
                    .FirstAsync(s => s.ProviderSportKey == entry.ProviderSportKey, ct);
                existing.Update(entry.Title, entry.Group, entry.Active, entry.HasOutrights,
                    entry.NormalizedSport, entry.NormalizedLeague, capturedAtUtc);
            }
            else
            {
                var sc = SportCatalog.Create(entry.ProviderSportKey, entry.Title, entry.Group,
                    entry.Active, entry.HasOutrights, entry.NormalizedSport, entry.NormalizedLeague,
                    capturedAtUtc);
                db.SportCatalogs.Add(sc);
            }
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SportCatalogEntry>> GetSportCatalogAsync(CancellationToken ct)
    {
        return await db.SportCatalogs
            .AsNoTracking()
            .OrderBy(s => s.Group).ThenBy(s => s.Title)
            .Select(s => new SportCatalogEntry(
                s.ProviderSportKey, s.Title, s.Group, s.Active, s.HasOutrights,
                s.NormalizedSport, s.NormalizedLeague, s.CapturedAtUtc))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TrackedLeagueInfo>> GetTrackedLeaguesAsync(CancellationToken ct)
    {
        return await db.TrackedLeagues
            .AsNoTracking()
            .OrderBy(t => t.ProviderSportKey)
            .Select(t => new TrackedLeagueInfo(
                t.Id, t.Provider, t.ProviderSportKey, t.Enabled, t.CreatedAtUtc, t.UpdatedAtUtc))
            .ToListAsync(ct);
    }

    public async Task ToggleTrackedLeagueAsync(string provider, string providerSportKey, bool enabled, DateTimeOffset now, CancellationToken ct)
    {
        var existing = await db.TrackedLeagues
            .FirstOrDefaultAsync(t => t.Provider == provider && t.ProviderSportKey == providerSportKey, ct);

        if (existing is not null)
        {
            existing.SetEnabled(enabled, now);
        }
        else
        {
            var tracked = TrackedLeague.Create(provider, providerSportKey, enabled, now);
            db.TrackedLeagues.Add(tracked);
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<Guid> UpsertEventAsync(string providerEventId, string sportKey, string sportTitle, string homeTeam, string awayTeam, DateTimeOffset commenceTimeUtc, DateTimeOffset now, CancellationToken ct)
    {
        var existing = await db.SportEvents
            .FirstOrDefaultAsync(e => e.ProviderEventId == providerEventId, ct);

        if (existing is not null)
        {
            existing.Update(sportTitle, homeTeam, awayTeam, commenceTimeUtc, now);
            await db.SaveChangesAsync(ct);
            return existing.Id;
        }

        var evt = SportEvent.Create(providerEventId, sportKey, sportTitle, homeTeam, awayTeam, commenceTimeUtc, now);
        db.SportEvents.Add(evt);
        await db.SaveChangesAsync(ct);
        return evt.Id;
    }

    public async Task AddOddsSnapshotsAsync(IReadOnlyList<OddsSnapshotEntry> snapshots, CancellationToken ct)
    {
        var entities = snapshots.Select(s =>
            OddsSnapshot.Create(s.EventId, s.BookmakerKey, s.BookmakerTitle,
                s.MarketKey, s.OutcomeName, s.Price, s.Point,
                s.ProviderLastUpdate, s.CapturedAtUtc));

        db.OddsSnapshots.AddRange(entities);
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
    }

    public async Task<IReadOnlyList<TrackedLeagueState>> GetTrackedLeagueStatesAsync(CancellationToken ct)
    {
        var trackedLeagues = await db.TrackedLeagues
            .AsNoTracking()
            .Where(t => t.Enabled)
            .ToListAsync(ct);

        var activeSportKeys = await db.SportCatalogs
            .AsNoTracking()
            .Where(sc => sc.Active)
            .Select(sc => sc.ProviderSportKey)
            .ToListAsync(ct);
        var activeSet = new HashSet<string>(activeSportKeys);

        var result = new List<TrackedLeagueState>();
        foreach (var tl in trackedLeagues)
        {
            var earliestEvent = await db.SportEvents
                .AsNoTracking()
                .Where(e => e.SportKey == tl.ProviderSportKey && e.CommenceTimeUtc > DateTimeOffset.UtcNow)
                .OrderBy(e => e.CommenceTimeUtc)
                .Select(e => (DateTimeOffset?)e.CommenceTimeUtc)
                .FirstOrDefaultAsync(ct);

            var latestSnapshot = await db.OddsSnapshots
                .AsNoTracking()
                .Where(s => db.SportEvents
                    .Where(e => e.SportKey == tl.ProviderSportKey)
                    .Select(e => e.Id)
                    .Contains(s.SportEventId))
                .OrderByDescending(s => s.CapturedAtUtc)
                .Select(s => (DateTimeOffset?)s.CapturedAtUtc)
                .FirstOrDefaultAsync(ct);

            result.Add(new TrackedLeagueState(
                tl.Provider, tl.ProviderSportKey, tl.Enabled,
                activeSet.Contains(tl.ProviderSportKey),
                earliestEvent, latestSnapshot));
        }

        return result;
    }

    public async Task<IReadOnlyList<IngestRunSummary>> GetRecentRunsAsync(int limit, CancellationToken ct)
    {
        return await db.IngestRuns
            .AsNoTracking()
            .OrderByDescending(r => r.StartedAtUtc)
            .Take(limit)
            .Select(r => new IngestRunSummary(
                r.Id, r.RunType, r.Status, r.StartedAtUtc, r.CompletedAtUtc,
                r.RequestCount, r.EventCount, r.SnapshotCount, r.ErrorCount))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SnapshotWithEvent>> GetLatestSnapshotsAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var rows = await (
            from s in db.OddsSnapshots.AsNoTracking()
            join e in db.SportEvents.AsNoTracking() on s.SportEventId equals e.Id
            where e.CommenceTimeUtc > now
            orderby s.CapturedAtUtc descending
            select new SnapshotWithEvent(
                e.Id, e.HomeTeam, e.AwayTeam, e.SportKey, e.SportTitle,
                e.CommenceTimeUtc,
                s.BookmakerKey, s.BookmakerTitle, s.MarketKey, s.OutcomeName,
                s.Price, s.Point, s.CapturedAtUtc)
        ).ToListAsync(ct);

        // Deduplicate: keep latest per (event, market, outcome, book)
        return rows
            .GroupBy(r => (r.SportEventId, r.MarketKey, r.OutcomeName, r.BookmakerKey))
            .Select(g => g.First()) // already ordered by CapturedAtUtc desc
            .ToList();
    }

    public async Task<IngestRunDetail?> GetRunDetailAsync(Guid runId, CancellationToken ct)
    {
        var run = await db.IngestRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == runId, ct);

        if (run is null) return null;

        var logs = await db.IngestLogs
            .AsNoTracking()
            .Where(l => l.IngestRunId == runId)
            .OrderBy(l => l.CreatedAtUtc)
            .Select(l => new IngestLogEntry(l.Level, l.Message, l.CreatedAtUtc))
            .ToListAsync(ct);

        return new IngestRunDetail(
            run.Id, run.RunType, run.Status, run.StartedAtUtc, run.CompletedAtUtc,
            run.Summary, run.RequestCount, run.EventCount, run.SnapshotCount, run.ErrorCount,
            logs);
    }
}
