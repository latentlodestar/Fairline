using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Fairline.Abstractions.Interfaces;
using Fairline.Infrastructure.Persistence;
using Fairline.Infrastructure.Persistence.Repositories;
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
        builder.Services.AddScoped<IDbContextChecker, DbContextChecker>();
        builder.Services.AddSingleton<IClock, SystemClock>();

        return builder;
    }
}
