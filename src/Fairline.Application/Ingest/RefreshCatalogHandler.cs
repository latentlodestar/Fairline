using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;
using Fairline.Domain.Ingest;

namespace Fairline.Application.Ingest;

public sealed class RefreshCatalogHandler(
    IOddsApiClient oddsApiClient,
    IIngestRepository repository,
    IClock clock)
{
    public async Task<CatalogRefreshResult> HandleAsync(CancellationToken ct = default)
    {
        var now = clock.UtcNow;
        var runId = await repository.CreateRunAsync(IngestRun.RunTypes.CatalogRefresh, now, ct);

        try
        {
            await repository.AddLogAsync(runId, IngestLog.Levels.Info, "Fetching sports catalog from Odds API", now, ct);

            var (sports, rawJson) = await oddsApiClient.GetSportsAsync(ct);

            await repository.SaveCatalogSnapshotAsync("the-odds-api", rawJson, sports.Count, now, ct);
            await repository.AddProviderRequestAsync(runId, "/v4/sports", 200, null, 0, null, now, ct);

            var normalized = CatalogNormalizer.Normalize(sports, now);
            await repository.UpsertSportCatalogAsync(normalized, now, ct);

            await repository.AddLogAsync(runId, IngestLog.Levels.Info, $"Catalog refreshed: {sports.Count} sports", clock.UtcNow, ct);
            await repository.CompleteRunAsync(runId, 1, 0, 0, 0, clock.UtcNow, ct);

            return new CatalogRefreshResult(sports.Count, normalized);
        }
        catch (Exception ex)
        {
            await repository.AddLogAsync(runId, IngestLog.Levels.Error, $"Catalog refresh failed: {ex.Message}", clock.UtcNow, ct);
            await repository.FailRunAsync(runId, ex.Message, clock.UtcNow, ct);
            throw;
        }
    }
}
