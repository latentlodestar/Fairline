using Fairline.Abstractions.Contracts;
using Fairline.Application.Ingest;
using Fairline.Domain.Ingest;
using Fairline.Infrastructure.Services;

namespace Fairline.Api.Endpoints;

public static class IngestEndpoints
{
    public static WebApplication MapIngestEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/ingest").WithTags("Ingest");

        group.MapGet("/providers", async (GetProvidersHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(ct)))
            .WithName("GetProviders");

        group.MapPost("/catalog/refresh", async (RefreshCatalogHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(ct)))
            .WithName("RefreshCatalog");

        group.MapGet("/catalog", async (GetCatalogHandler handler, CancellationToken ct) =>
        {
            var (sports, tracked) = await handler.HandleAsync(ct);
            return Results.Ok(new { sports, trackedLeagues = tracked });
        })
            .WithName("GetCatalog");

        group.MapPost("/catalog/track", async (ToggleTrackedLeagueRequest request, ToggleTrackedLeagueHandler handler, CancellationToken ct) =>
        {
            await handler.HandleAsync(request, ct);
            return Results.Ok();
        })
            .WithName("ToggleTrackedLeague");

        group.MapPost("/run", async (
            RunIngestionRequest? request,
            IServiceScopeFactory scopeFactory,
            IngestRunNotifier notifier,
            Fairline.Abstractions.Interfaces.IIngestRepository repository,
            Fairline.Abstractions.Interfaces.IClock clock) =>
        {
            request ??= new RunIngestionRequest();
            var now = clock.UtcNow;
            var runId = await repository.CreateRunAsync(IngestRun.RunTypes.GapFill, now);

            notifier.CreateChannel(runId);
            var cancellationToken = notifier.CreateCancellation(runId);

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<RunGapFillIngestionHandler>();
                    await handler.HandleAsync(runId, request, cancellationToken);
                }
                catch (Exception ex)
                {
                    notifier.Publish(runId, "log",
                        System.Text.Json.JsonSerializer.Serialize(new { level = "Error", message = ex.Message, timestamp = DateTimeOffset.UtcNow }));
                }
                finally
                {
                    notifier.Complete(runId);
                }
            });

            return Results.Ok(new { runId });
        })
            .WithName("RunIngestion");

        group.MapPost("/runs/{runId:guid}/cancel", async (
            Guid runId,
            IngestRunNotifier notifier,
            Fairline.Abstractions.Interfaces.IIngestRepository repository,
            Fairline.Abstractions.Interfaces.IClock clock) =>
        {
            await repository.CancelRunAsync(runId, clock.UtcNow);
            notifier.RequestCancellation(runId);
            return Results.Ok();
        })
            .WithName("CancelRun");

        group.MapGet("/runs", async (GetRunsHandler handler, int? limit, CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(limit ?? 20, ct)))
            .WithName("GetRuns");

        group.MapGet("/runs/{runId:guid}", async (Guid runId, GetRunDetailHandler handler, CancellationToken ct) =>
        {
            var detail = await handler.HandleAsync(runId, ct);
            return detail is null ? Results.NotFound() : Results.Ok(detail);
        })
            .WithName("GetRunDetail");

        group.MapGet("/runs/{runId:guid}/stream", async (
            Guid runId,
            IngestRunNotifier notifier,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var reader = notifier.TryGetReader(runId);
            if (reader is null)
            {
                httpContext.Response.StatusCode = 404;
                return;
            }

            httpContext.Response.ContentType = "text/event-stream";
            httpContext.Response.Headers.CacheControl = "no-cache";

            try
            {
                await foreach (var evt in reader.ReadAllAsync(ct))
                {
                    await httpContext.Response.WriteAsync($"event: {evt.EventType}\ndata: {evt.Data}\n\n", ct);
                    await httpContext.Response.Body.FlushAsync(ct);
                }
            }
            catch (OperationCanceledException)
            {
                // Client disconnected
            }
        })
            .WithName("StreamRunEvents");

        return app;
    }
}
