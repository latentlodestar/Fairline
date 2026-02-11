using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fairline.Domain.Ingest;

namespace Fairline.Infrastructure.Persistence.Configurations.Ingest;

public sealed class IngestRunConfiguration : IEntityTypeConfiguration<IngestRun>
{
    public void Configure(EntityTypeBuilder<IngestRun> builder)
    {
        builder.ToTable("ingest_runs");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.RunType).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Status).HasMaxLength(50).IsRequired();
        builder.Property(r => r.StartedAtUtc).IsRequired();
        builder.Property(r => r.CompletedAtUtc);
        builder.Property(r => r.Summary).HasMaxLength(4000);
        builder.Property(r => r.RequestCount).IsRequired();
        builder.Property(r => r.EventCount).IsRequired();
        builder.Property(r => r.SnapshotCount).IsRequired();
        builder.Property(r => r.ErrorCount).IsRequired();

        builder.HasIndex(r => r.StartedAtUtc).IsDescending();
    }
}
