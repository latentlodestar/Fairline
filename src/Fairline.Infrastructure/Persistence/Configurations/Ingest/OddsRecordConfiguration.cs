using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fairline.Domain.Ingest;

namespace Fairline.Infrastructure.Persistence.Configurations.Ingest;

public sealed class OddsRecordConfiguration : IEntityTypeConfiguration<OddsRecord>
{
    public void Configure(EntityTypeBuilder<OddsRecord> builder)
    {
        builder.ToTable("odds_records");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.ProviderId).IsRequired();
        builder.Property(r => r.EventKey).HasMaxLength(300).IsRequired();
        builder.Property(r => r.Market).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Selection).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Odds).HasPrecision(18, 6).IsRequired();
        builder.Property(r => r.RawPayload).HasColumnType("jsonb");
        builder.Property(r => r.IngestedAt).IsRequired();

        builder.HasIndex(r => r.ProviderId);
        builder.HasIndex(r => new { r.EventKey, r.Market });

        builder.HasOne<Provider>()
            .WithMany()
            .HasForeignKey(r => r.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
