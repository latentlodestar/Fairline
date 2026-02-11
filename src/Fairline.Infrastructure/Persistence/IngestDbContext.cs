using Microsoft.EntityFrameworkCore;
using Fairline.Domain.Ingest;

namespace Fairline.Infrastructure.Persistence;

public sealed class IngestDbContext(DbContextOptions<IngestDbContext> options) : DbContext(options)
{
    public const string Schema = "ingest";

    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<OddsRecord> OddsRecords => Set<OddsRecord>();
    public DbSet<IngestRun> IngestRuns => Set<IngestRun>();
    public DbSet<IngestLog> IngestLogs => Set<IngestLog>();
    public DbSet<ProviderRequest> ProviderRequests => Set<ProviderRequest>();
    public DbSet<SportEvent> SportEvents => Set<SportEvent>();
    public DbSet<OddsSnapshot> OddsSnapshots => Set<OddsSnapshot>();
    public DbSet<ProviderCatalogSnapshot> ProviderCatalogSnapshots => Set<ProviderCatalogSnapshot>();
    public DbSet<SportCatalog> SportCatalogs => Set<SportCatalog>();
    public DbSet<TrackedLeague> TrackedLeagues => Set<TrackedLeague>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IngestDbContext).Assembly,
            type => type.Namespace?.Contains("Configurations.Ingest") == true);
    }
}
