using Fairline.Abstractions.Contracts;

namespace Fairline.Abstractions.Interfaces;

public interface IIngestRepository
{
    // Runs
    Task<Guid> CreateRunAsync(string runType, DateTimeOffset startedAtUtc, CancellationToken ct = default);
    Task CompleteRunAsync(Guid runId, int requestCount, int eventCount, int snapshotCount, int errorCount, DateTimeOffset completedAtUtc, CancellationToken ct = default);
    Task FailRunAsync(Guid runId, string error, DateTimeOffset now, CancellationToken ct = default);
    Task CancelRunAsync(Guid runId, DateTimeOffset now, CancellationToken ct = default);
    Task CancelRunAsync(Guid runId, int requestCount, int eventCount, int snapshotCount, DateTimeOffset now, CancellationToken ct = default);
    Task SetRunSummaryAsync(Guid runId, string summary, CancellationToken ct = default);
    Task AddLogAsync(Guid runId, string level, string message, DateTimeOffset createdAtUtc, CancellationToken ct = default);
    Task AddProviderRequestAsync(Guid runId, string url, int? statusCode, long? durationMs, int? quotaUsed, string? errorMessage, DateTimeOffset requestedAtUtc, CancellationToken ct = default);

    // Catalog
    Task SaveCatalogSnapshotAsync(string provider, string rawJson, int sportCount, DateTimeOffset capturedAtUtc, CancellationToken ct = default);
    Task UpsertSportCatalogAsync(IReadOnlyList<SportCatalogEntry> entries, DateTimeOffset capturedAtUtc, CancellationToken ct = default);
    Task<IReadOnlyList<SportCatalogEntry>> GetSportCatalogAsync(CancellationToken ct = default);

    // Tracked leagues
    Task<IReadOnlyList<TrackedLeagueInfo>> GetTrackedLeaguesAsync(CancellationToken ct = default);
    Task ToggleTrackedLeagueAsync(string provider, string providerSportKey, bool enabled, DateTimeOffset now, CancellationToken ct = default);

    // Events & Odds
    Task<Guid> UpsertEventAsync(string providerEventId, string sportKey, string sportTitle, string homeTeam, string awayTeam, DateTimeOffset commenceTimeUtc, DateTimeOffset now, CancellationToken ct = default);
    Task AddOddsSnapshotsAsync(IReadOnlyList<OddsSnapshotEntry> snapshots, CancellationToken ct = default);

    // Gap planning
    Task<IReadOnlyList<TrackedLeagueState>> GetTrackedLeagueStatesAsync(CancellationToken ct = default);

    // Queries
    Task<IReadOnlyList<IngestRunSummary>> GetRecentRunsAsync(int limit = 20, CancellationToken ct = default);
    Task<IngestRunDetail?> GetRunDetailAsync(Guid runId, CancellationToken ct = default);

    // Dashboard
    Task<IReadOnlyList<SnapshotWithEvent>> GetLatestSnapshotsAsync(CancellationToken ct = default);
}
