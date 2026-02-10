using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fairline.Domain.Modeling;

namespace Fairline.Infrastructure.Persistence.Configurations.Modeling;

public sealed class ScenarioConfiguration : IEntityTypeConfiguration<Scenario>
{
    public void Configure(EntityTypeBuilder<Scenario> builder)
    {
        builder.ToTable("scenarios");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Name).HasMaxLength(300).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(2000);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasMany(s => s.Comparisons)
            .WithOne()
            .HasForeignKey(c => c.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Comparisons).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
