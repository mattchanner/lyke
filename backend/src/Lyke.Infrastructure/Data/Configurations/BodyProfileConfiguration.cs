using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class BodyProfileConfiguration : IEntityTypeConfiguration<BodyProfile>
{
    public void Configure(EntityTypeBuilder<BodyProfile> builder)
    {
        builder.HasKey(bp => bp.Id);

        builder.Property(bp => bp.HeightCm)
            .IsRequired();

        builder.Property(bp => bp.WeightKg)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(bp => bp.FitPreference)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(bp => bp.BodyType)
            .WithMany(bt => bt.BodyProfiles)
            .HasForeignKey(bp => bp.BodyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bp => new { bp.BodyTypeId, bp.HeightCm, bp.WeightKg })
            .HasDatabaseName("IX_BodyProfiles_BodyTypeId_HeightCm_WeightKg");
    }
}
