using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;

namespace Fairline.Application.Ingest;

public sealed class GetRunsHandler(IIngestRepository repository)
{
    public Task<IReadOnlyList<IngestRunSummary>> HandleAsync(int limit = 20, CancellationToken ct = default)
        => repository.GetRecentRunsAsync(limit, ct);
}
