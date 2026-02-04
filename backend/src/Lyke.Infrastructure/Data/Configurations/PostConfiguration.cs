using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.MediaType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.MediaUrls)
            .HasColumnType("jsonb");

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.ModerationNotes)
            .HasMaxLength(1000);

        // Moderation tracking
        builder.HasOne(p => p.ModeratedByUser)
            .WithMany()
            .HasForeignKey(p => p.ModeratedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => new { p.CreatorId, p.Status, p.PublishedAt })
            .HasDatabaseName("IX_Posts_CreatorId_Status_PublishedAt");

        builder.HasIndex(p => new { p.Status, p.PublishedAt })
            .HasDatabaseName("IX_Posts_Status_PublishedAt");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Posts_Status");
    }
}
