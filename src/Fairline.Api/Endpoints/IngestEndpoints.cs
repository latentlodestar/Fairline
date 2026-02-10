using Fairline.Application.Ingest;

namespace Fairline.Api.Endpoints;

public static class IngestEndpoints
{
    public static WebApplication MapIngestEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/ingest").WithTags("Ingest");

        group.MapGet("/providers", async (GetProvidersHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(ct)))
            .WithName("GetProviders");

        return app;
    }
}
