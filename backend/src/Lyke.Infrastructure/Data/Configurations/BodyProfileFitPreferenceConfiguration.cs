using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class BodyProfileFitPreferenceConfiguration
    : IEntityTypeConfiguration<BodyProfileFitPreference>
{
    public void Configure(EntityTypeBuilder<BodyProfileFitPreference> builder)
    {
        builder.HasKey(bpfp => new { bpfp.BodyProfileId, bpfp.FitPreference });

        builder.Property(bpfp => bpfp.FitPreference).HasConversion<string>().HasMaxLength(20);

        builder
            .HasOne(bpfp => bpfp.BodyProfile)
            .WithMany(bp => bp.FitPreferences)
            .HasForeignKey(bpfp => bpfp.BodyProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
