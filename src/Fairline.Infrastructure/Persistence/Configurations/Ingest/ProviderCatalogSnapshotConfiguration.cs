using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fairline.Domain.Ingest;

namespace Fairline.Infrastructure.Persistence.Configurations.Ingest;

public sealed class ProviderCatalogSnapshotConfiguration : IEntityTypeConfiguration<ProviderCatalogSnapshot>
{
    public void Configure(EntityTypeBuilder<ProviderCatalogSnapshot> builder)
    {
        builder.ToTable("provider_catalog_snapshots");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Provider).HasMaxLength(100).IsRequired();
        builder.Property(s => s.RawJson).HasColumnType("jsonb").IsRequired();
        builder.Property(s => s.SportCount).IsRequired();
        builder.Property(s => s.CapturedAtUtc).IsRequired();

        builder.HasIndex(s => s.CapturedAtUtc).IsDescending();
    }
}
