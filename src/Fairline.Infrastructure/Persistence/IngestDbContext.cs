using Microsoft.EntityFrameworkCore;
using Fairline.Domain.Ingest;

namespace Fairline.Infrastructure.Persistence;

public sealed class IngestDbContext(DbContextOptions<IngestDbContext> options) : DbContext(options)
{
    public const string Schema = "ingest";

    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<OddsRecord> OddsRecords => Set<OddsRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IngestDbContext).Assembly,
            type => type.Namespace?.Contains("Configurations.Ingest") == true);
    }
}
