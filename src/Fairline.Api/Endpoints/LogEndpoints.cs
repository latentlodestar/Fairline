using Microsoft.Extensions.Logging;

namespace Fairline.Api.Endpoints;

public static class LogEndpoints
{
    public static WebApplication MapLogEndpoints(this WebApplication app)
    {
        app.MapPost("/api/log", (FrontendLogEntry[] entries, ILoggerFactory loggerFactory) =>
        {
            foreach (var entry in entries)
            {
                var logger = loggerFactory.CreateLogger($"Frontend.{entry.Source}");
                var level = MapLevel(entry.Level);
                logger.Log(level, "{Message} {Data}", entry.Message, entry.Data);
            }

            return Results.NoContent();
        })
        .WithName("PostFrontendLogs")
        .WithTags("Logging");

        return app;
    }

    private static LogLevel MapLevel(string level) => level.ToLowerInvariant() switch
    {
        "debug" => LogLevel.Debug,
        "info" => LogLevel.Information,
        "warn" => LogLevel.Warning,
        "error" => LogLevel.Error,
        _ => LogLevel.Information,
    };
}

public record FrontendLogEntry(string Source, string Level, string Message, string? Data = null);
