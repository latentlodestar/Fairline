namespace Fairline.Abstractions.Contracts;

// Raw row from repository (pre-computation)
public sealed record SnapshotWithEvent(
    Guid SportEventId, string HomeTeam, string AwayTeam,
    string SportKey, string SportTitle,
    string BookmakerKey, string BookmakerTitle,
    string MarketKey, string OutcomeName,
    decimal Price, decimal? Point, DateTimeOffset CapturedAtUtc);

// Response DTOs
public sealed record DashboardResponse(DashboardKpis Kpis, IReadOnlyList<EdgeRow> Edges);

public sealed record DashboardKpis(
    int EventCount, int SnapshotCount, int BookCount,
    DateTimeOffset? LatestCaptureUtc);

public sealed record EdgeRow(
    Guid SportEventId, string HomeTeam, string AwayTeam,
    string SportKey, string SportTitle,
    string MarketKey, string OutcomeName,
    string BookmakerKey, string BookmakerTitle,
    decimal FairLine, decimal BookLine, decimal EdgePct, string Signal);

// Edge comparison model — one row per (event, market, selection)
public sealed record EdgeComparisonRow(
    Guid EventId,
    string HomeTeam,
    string AwayTeam,
    string SportKey,
    string SportTitle,
    string MarketType,
    string SelectionKey,
    decimal? BaselinePrice,
    decimal? BaselinePoint,
    decimal? BaselineDecimal,
    string? BaselineBook,
    decimal? TargetPrice,
    decimal? TargetPoint,
    decimal? TargetDecimal,
    string TargetBook,
    decimal? EdgePct,
    string Signal,
    DateTimeOffset LastUpdatedUtc);

public sealed record EdgeComparisonsResponse(
    DashboardKpis Kpis,
    IReadOnlyList<EdgeComparisonRow> Comparisons);
