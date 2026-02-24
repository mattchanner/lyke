using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class FrameSizeConfiguration : IEntityTypeConfiguration<FrameSize>
{
    public void Configure(EntityTypeBuilder<FrameSize> builder)
    {
        builder.HasKey(fs => fs.Id);

        builder.Property(fs => fs.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(fs => fs.Description)
            .HasMaxLength(500);

        builder.HasData(
            new FrameSize { Id = 1, Name = "Petite", Description = "Shorter stature with a smaller overall frame", DisplayOrder = 1 },
            new FrameSize { Id = 2, Name = "Average", Description = "Medium height and proportional build", DisplayOrder = 2 },
            new FrameSize { Id = 3, Name = "Tall", Description = "Taller stature with a longer frame", DisplayOrder = 3 },
            new FrameSize { Id = 4, Name = "Plus", Description = "Fuller figure across all areas", DisplayOrder = 4 }
        );
    }
}
