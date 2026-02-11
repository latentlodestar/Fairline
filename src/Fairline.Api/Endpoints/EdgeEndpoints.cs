using Fairline.Application.Dashboard;

namespace Fairline.Api.Endpoints;

public static class EdgeEndpoints
{
    public static WebApplication MapEdgeEndpoints(this WebApplication app)
    {
        app.MapGet("/api/edges/comparisons", async (
                GetEdgeComparisonsHandler handler,
                string? baseline,
                string? target,
                bool? showIncomplete,
                CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(
                baseline ?? "pinnacle",
                target ?? "draftkings",
                showIncomplete ?? false,
                ct)))
            .WithName("GetEdgeComparisons")
            .WithTags("Edges");

        return app;
    }
}
