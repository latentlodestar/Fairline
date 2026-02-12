namespace Fairline.Abstractions.Contracts;

public sealed record SportCatalogEntry(
    string ProviderSportKey,
    string Title,
    string Group,
    bool Active,
    bool HasOutrights,
    string NormalizedSport,
    string NormalizedLeague,
    DateTimeOffset CapturedAtUtc);

public sealed record TrackedLeagueInfo(
    Guid Id,
    string Provider,
    string ProviderSportKey,
    bool Enabled,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record IngestRunSummary(
    Guid Id,
    string RunType,
    string Status,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    int RequestCount,
    int EventCount,
    int SnapshotCount,
    int ErrorCount);

public sealed record IngestRunDetail(
    Guid Id,
    string RunType,
    string Status,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? Summary,
    int RequestCount,
    int EventCount,
    int SnapshotCount,
    int ErrorCount,
    IReadOnlyList<IngestLogEntry> Logs);

public sealed record IngestLogEntry(
    string Level,
    string Message,
    DateTimeOffset CreatedAtUtc);

public sealed record OddsSnapshotEntry(
    Guid EventId,
    string BookmakerKey,
    string BookmakerTitle,
    string MarketKey,
    string OutcomeName,
    decimal Price,
    decimal? Point,
    DateTimeOffset? ProviderLastUpdate,
    DateTimeOffset CapturedAtUtc);

public sealed record TrackedLeagueState(
    string Provider,
    string ProviderSportKey,
    bool Enabled,
    bool Active,
    bool HasOutrights,
    DateTimeOffset? EarliestEventCommenceTime,
    DateTimeOffset? LatestSnapshotCapturedAtUtc);

public sealed record CatalogRefreshResult(
    int SportCount,
    IReadOnlyList<SportCatalogEntry> Sports);

public sealed record RunIngestionRequest(
    int WindowHours = 72,
    string[] Regions = null!,
    string[] Markets = null!,
    string[]? Books = null)
{
    public string[] Regions { get; init; } = Regions ?? ["us"];
    public string[] Markets { get; init; } = Markets ?? ["h2h", "spreads", "totals"];
}

public sealed record ToggleTrackedLeagueRequest(
    string ProviderSportKey,
    bool Enabled);
