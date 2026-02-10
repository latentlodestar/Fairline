using Fairline.Application.Status;

namespace Fairline.Api.Endpoints;

public static class StatusEndpoints
{
    public static WebApplication MapStatusEndpoints(this WebApplication app)
    {
        app.MapGet("/api/status", async (GetStatusHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(ct)))
            .WithName("GetStatus")
            .WithTags("Status");

        return app;
    }
}
