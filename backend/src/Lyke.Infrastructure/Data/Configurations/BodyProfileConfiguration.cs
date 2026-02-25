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

        builder.Property(bp => bp.Stature)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(bp => bp.Build)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(bp => bp.BodyType)
            .WithMany(bt => bt.BodyProfiles)
            .HasForeignKey(bp => bp.BodyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bp => bp.FrameSize)
            .WithMany(fs => fs.BodyProfiles)
            .HasForeignKey(bp => bp.FrameSizeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bp => new { bp.BodyTypeId, bp.FrameSizeId, bp.HeightCm, bp.WeightKg })
            .HasDatabaseName("IX_BodyProfiles_BodyType_FrameSize_Height_Weight");
    }
}
