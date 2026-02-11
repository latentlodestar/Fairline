using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fairline.Domain.Ingest;

namespace Fairline.Infrastructure.Persistence.Configurations.Ingest;

public sealed class IngestLogConfiguration : IEntityTypeConfiguration<IngestLog>
{
    public void Configure(EntityTypeBuilder<IngestLog> builder)
    {
        builder.ToTable("ingest_logs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.IngestRunId).IsRequired();
        builder.Property(l => l.Level).HasMaxLength(20).IsRequired();
        builder.Property(l => l.Message).HasMaxLength(4000).IsRequired();
        builder.Property(l => l.CreatedAtUtc).IsRequired();

        builder.HasIndex(l => l.IngestRunId);
        builder.HasIndex(l => new { l.IngestRunId, l.CreatedAtUtc });
    }
}
