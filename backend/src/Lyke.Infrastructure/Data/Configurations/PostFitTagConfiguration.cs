using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class PostFitTagConfiguration : IEntityTypeConfiguration<PostFitTag>
{
    public void Configure(EntityTypeBuilder<PostFitTag> builder)
    {
        builder.HasKey(pft => new { pft.PostProductId, pft.FitTagId });

        builder.HasOne(pft => pft.PostProduct)
            .WithMany(pp => pp.FitTags)
            .HasForeignKey(pft => pft.PostProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pft => pft.FitTag)
            .WithMany(ft => ft.PostFitTags)
            .HasForeignKey(pft => pft.FitTagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
