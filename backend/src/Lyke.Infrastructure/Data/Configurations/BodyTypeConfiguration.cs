using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class BodyTypeConfiguration : IEntityTypeConfiguration<BodyType>
{
    public void Configure(EntityTypeBuilder<BodyType> builder)
    {
        builder.HasKey(bt => bt.Id);

        builder.Property(bt => bt.Name).HasMaxLength(100).IsRequired();

        builder.Property(bt => bt.Description).HasMaxLength(500);

        // Body shapes only — frame/size attributes are now in FrameSize
        builder.HasData(
            new BodyType
            {
                Id = 1,
                Name = "Hourglass",
                Description = "Balanced bust and hips with defined waist",
                DisplayOrder = 1,
            },
            new BodyType
            {
                Id = 2,
                Name = "Pear",
                Description = "Hips wider than shoulders",
                DisplayOrder = 2,
            },
            new BodyType
            {
                Id = 3,
                Name = "Apple",
                Description = "Fuller midsection with slimmer legs",
                DisplayOrder = 3,
            },
            new BodyType
            {
                Id = 4,
                Name = "Rectangle",
                Description = "Balanced proportions, less waist definition",
                DisplayOrder = 4,
            },
            new BodyType
            {
                Id = 5,
                Name = "Inverted Triangle",
                Description = "Shoulders wider than hips",
                DisplayOrder = 5,
            }
        );
    }
}
