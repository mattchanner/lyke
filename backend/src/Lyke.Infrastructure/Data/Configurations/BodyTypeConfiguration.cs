using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class BodyTypeConfiguration : IEntityTypeConfiguration<BodyType>
{
    public void Configure(EntityTypeBuilder<BodyType> builder)
    {
        builder.HasKey(bt => bt.Id);

        builder.Property(bt => bt.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(bt => bt.Description)
            .HasMaxLength(500);

        builder.HasData(
            new BodyType { Id = 1, Name = "Petite", Description = "Shorter stature with proportional frame", DisplayOrder = 1 },
            new BodyType { Id = 2, Name = "Slim", Description = "Lean build with narrow shoulders and hips", DisplayOrder = 2 },
            new BodyType { Id = 3, Name = "Athletic", Description = "Muscular build with broader shoulders", DisplayOrder = 3 },
            new BodyType { Id = 4, Name = "Hourglass", Description = "Balanced bust and hips with defined waist", DisplayOrder = 4 },
            new BodyType { Id = 5, Name = "Pear", Description = "Hips wider than shoulders", DisplayOrder = 5 },
            new BodyType { Id = 6, Name = "Apple", Description = "Fuller midsection with slimmer legs", DisplayOrder = 6 },
            new BodyType { Id = 7, Name = "Rectangle", Description = "Balanced proportions throughout", DisplayOrder = 7 },
            new BodyType { Id = 8, Name = "Plus Size", Description = "Fuller figure across all areas", DisplayOrder = 8 }
        );
    }
}
