using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class StyleProfileConfiguration : IEntityTypeConfiguration<StyleProfile>
{
    public void Configure(EntityTypeBuilder<StyleProfile> builder)
    {
        builder.ToTable("style_profiles");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId).IsUnique();

        // Enum conversions to string for readability
        builder.Property(x => x.PrimaryFamily).HasConversion<string>().HasMaxLength(20);

        builder.Property(x => x.RunnerUpFamily).HasConversion<string>().HasMaxLength(20);

        builder.Property(x => x.Confidence).HasConversion<string>().HasMaxLength(20);

        builder.Property(x => x.BoneDominance).HasConversion<string>().HasMaxLength(20);

        builder.Property(x => x.FleshDominance).HasConversion<string>().HasMaxLength(20);

        builder.Property(x => x.FaceDominance).HasConversion<string>().HasMaxLength(20);

        builder.Property(x => x.OverrideFamily).HasConversion<string>().HasMaxLength(20);

        // JSON column for raw counts
        builder.Property(x => x.CountsJson).HasColumnType("jsonb");
    }
}
