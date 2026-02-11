using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;

namespace Fairline.Application.Ingest;

public sealed class ToggleTrackedLeagueHandler(IIngestRepository repository, IClock clock)
{
    public async Task HandleAsync(ToggleTrackedLeagueRequest request, CancellationToken ct = default)
    {
        await repository.ToggleTrackedLeagueAsync(
            "the-odds-api", request.ProviderSportKey, request.Enabled, clock.UtcNow, ct);
    }
}
