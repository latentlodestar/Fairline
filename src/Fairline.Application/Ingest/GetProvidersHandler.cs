using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;

namespace Fairline.Application.Ingest;

public sealed class GetProvidersHandler(IOddsRepository repository)
{
    public Task<IReadOnlyList<ProviderInfo>> HandleAsync(CancellationToken ct = default)
        => repository.GetProvidersAsync(ct);
}
