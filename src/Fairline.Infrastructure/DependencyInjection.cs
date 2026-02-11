using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Fairline.Abstractions.Interfaces;
using Fairline.Infrastructure.Persistence;
using Fairline.Infrastructure.Persistence.Repositories;
using Fairline.Infrastructure.Providers;
using Fairline.Infrastructure.Services;

namespace Fairline.Infrastructure;

public static class DependencyInjection
{
    public static TBuilder AddInfrastructure<TBuilder>(this TBuilder builder, string connectionName = "fairlinedb")
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddNpgsqlDbContext<IngestDbContext>(connectionName);
        builder.AddNpgsqlDbContext<ModelingDbContext>(connectionName);

        builder.Services.AddScoped<IOddsRepository, OddsRepository>();
        builder.Services.AddScoped<IScenarioRepository, ScenarioRepository>();
        builder.Services.AddScoped<IIngestRepository, IngestRepository>();
        builder.Services.AddScoped<IDbContextChecker, DbContextChecker>();
        builder.Services.AddSingleton<IClock, SystemClock>();

        // Odds API client
        builder.Services.Configure<OddsApiOptions>(builder.Configuration.GetSection(OddsApiOptions.SectionName));

        // Map ODDS_API_KEY env var to OddsApi:ApiKey if present
        var envKey = builder.Configuration["ODDS_API_KEY"];
        if (!string.IsNullOrEmpty(envKey))
        {
            builder.Configuration[OddsApiOptions.SectionName + ":ApiKey"] = envKey;
        }

        builder.Services.AddHttpClient<IOddsApiClient, OddsApiClient>();

        // SSE notifier (singleton for cross-scope event streaming)
        var notifier = new IngestRunNotifier();
        builder.Services.AddSingleton<IngestRunNotifier>(notifier);
        builder.Services.AddSingleton<IIngestEventSink>(notifier);

        return builder;
    }
}
