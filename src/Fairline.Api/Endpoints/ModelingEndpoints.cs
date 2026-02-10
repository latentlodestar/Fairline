using Fairline.Application.Modeling;

namespace Fairline.Api.Endpoints;

public static class ModelingEndpoints
{
    public static WebApplication MapModelingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/modeling").WithTags("Modeling");

        group.MapGet("/scenarios", async (GetScenariosHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(ct)))
            .WithName("GetScenarios");

        return app;
    }
}
