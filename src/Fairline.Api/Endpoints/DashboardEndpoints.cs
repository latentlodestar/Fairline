using Fairline.Application.Dashboard;

namespace Fairline.Api.Endpoints;

public static class DashboardEndpoints
{
    public static WebApplication MapDashboardEndpoints(this WebApplication app)
    {
        app.MapGet("/api/dashboard", async (GetDashboardHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(ct)))
            .WithName("GetDashboard")
            .WithTags("Dashboard");

        return app;
    }
}
