using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class MediaProcessingJobConfiguration : IEntityTypeConfiguration<MediaProcessingJob>
{
    public void Configure(EntityTypeBuilder<MediaProcessingJob> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.MediaId).IsRequired().HasMaxLength(32);

        builder.Property(e => e.ContentType).IsRequired().HasMaxLength(100);

        builder.Property(e => e.Status).IsRequired();

        builder
            .HasIndex(e => new { e.MediaId, e.UserId })
            .IsUnique()
            .HasDatabaseName("IX_MediaProcessingJobs_MediaId_UserId");

        builder.HasIndex(e => e.UserId).HasDatabaseName("IX_MediaProcessingJobs_UserId");
    }
}
