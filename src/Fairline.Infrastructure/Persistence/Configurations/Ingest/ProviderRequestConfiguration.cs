using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fairline.Domain.Ingest;

namespace Fairline.Infrastructure.Persistence.Configurations.Ingest;

public sealed class ProviderRequestConfiguration : IEntityTypeConfiguration<ProviderRequest>
{
    public void Configure(EntityTypeBuilder<ProviderRequest> builder)
    {
        builder.ToTable("provider_requests");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.IngestRunId).IsRequired();
        builder.Property(r => r.Url).HasMaxLength(2000).IsRequired();
        builder.Property(r => r.StatusCode);
        builder.Property(r => r.DurationMs);
        builder.Property(r => r.RequestedAtUtc).IsRequired();
        builder.Property(r => r.QuotaUsed);
        builder.Property(r => r.ErrorMessage).HasMaxLength(4000);

        builder.HasIndex(r => r.IngestRunId);
    }
}
