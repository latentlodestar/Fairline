using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;

namespace Fairline.Application.Ingest;

public sealed class GetRunDetailHandler(IIngestRepository repository)
{
    public Task<IngestRunDetail?> HandleAsync(Guid runId, CancellationToken ct = default)
        => repository.GetRunDetailAsync(runId, ct);
}
