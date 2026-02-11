using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;

namespace Fairline.Application.Ingest;

public sealed class GetCatalogHandler(IIngestRepository repository)
{
    public async Task<(IReadOnlyList<SportCatalogEntry> Sports, IReadOnlyList<TrackedLeagueInfo> TrackedLeagues)>
        HandleAsync(CancellationToken ct = default)
    {
        var sports = await repository.GetSportCatalogAsync(ct);
        var tracked = await repository.GetTrackedLeaguesAsync(ct);
        return (sports, tracked);
    }
}
