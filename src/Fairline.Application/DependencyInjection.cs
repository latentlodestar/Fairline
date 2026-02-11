using Microsoft.Extensions.DependencyInjection;
using Fairline.Application.Ingest;
using Fairline.Application.Modeling;
using Fairline.Application.Status;

namespace Fairline.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GetProvidersHandler>();
        services.AddScoped<GetScenariosHandler>();
        services.AddScoped<GetStatusHandler>();

        // Ingestion handlers
        services.AddScoped<RefreshCatalogHandler>();
        services.AddScoped<ToggleTrackedLeagueHandler>();
        services.AddScoped<RunGapFillIngestionHandler>();
        services.AddScoped<GetCatalogHandler>();
        services.AddScoped<GetRunsHandler>();
        services.AddScoped<GetRunDetailHandler>();

        return services;
    }
}
