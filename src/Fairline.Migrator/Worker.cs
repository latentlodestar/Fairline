using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Fairline.Infrastructure.Persistence;

namespace Fairline.Migrator;

public sealed class MigrationWorker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<MigrationWorker> logger) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            await MigrateContextAsync<IngestDbContext>(stoppingToken);
            await MigrateContextAsync<ModelingDbContext>(stoppingToken);

            logger.LogInformation("All database migrations completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database");
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private async Task MigrateContextAsync<TContext>(CancellationToken ct) where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;
        logger.LogInformation("Applying migrations for {Context}", contextName);

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await context.Database.MigrateAsync(ct);
        });

        logger.LogInformation("Migrations applied for {Context}", contextName);
    }
}
