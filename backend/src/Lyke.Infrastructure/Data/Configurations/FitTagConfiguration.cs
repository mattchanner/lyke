using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class FitTagConfiguration : IEntityTypeConfiguration<FitTag>
{
    public void Configure(EntityTypeBuilder<FitTag> builder)
    {
        builder.HasKey(ft => ft.Id);

        builder.Property(ft => ft.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ft => ft.Category)
            .HasMaxLength(50);

        builder.HasData(
            new FitTag { Id = 1, Name = "True to size", Category = "General", IsActive = true },
            new FitTag { Id = 2, Name = "Runs small", Category = "General", IsActive = true },
            new FitTag { Id = 3, Name = "Runs large", Category = "General", IsActive = true },
            new FitTag { Id = 4, Name = "Tight on hips", Category = "Fit", IsActive = true },
            new FitTag { Id = 5, Name = "Tight on bust", Category = "Fit", IsActive = true },
            new FitTag { Id = 6, Name = "Loose on waist", Category = "Fit", IsActive = true },
            new FitTag { Id = 7, Name = "Long in arms", Category = "Length", IsActive = true },
            new FitTag { Id = 8, Name = "Short in arms", Category = "Length", IsActive = true },
            new FitTag { Id = 9, Name = "Long in torso", Category = "Length", IsActive = true },
            new FitTag { Id = 10, Name = "Short in torso", Category = "Length", IsActive = true },
            new FitTag { Id = 11, Name = "Stretchy material", Category = "Material", IsActive = true },
            new FitTag { Id = 12, Name = "Not stretchy", Category = "Material", IsActive = true }
        );
    }
}
