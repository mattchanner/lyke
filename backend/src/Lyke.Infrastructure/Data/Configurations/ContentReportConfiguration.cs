using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class ContentReportConfiguration : IEntityTypeConfiguration<ContentReport>
{
    public void Configure(EntityTypeBuilder<ContentReport> builder)
    {
        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Reason).HasConversion<string>().HasMaxLength(50);

        builder.Property(cr => cr.Status).HasConversion<string>().HasMaxLength(50);

        builder.Property(cr => cr.AdditionalDetails).HasMaxLength(1000);

        builder.Property(cr => cr.ReviewNotes).HasMaxLength(1000);

        builder
            .HasOne(cr => cr.Post)
            .WithMany()
            .HasForeignKey(cr => cr.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(cr => cr.ReportedByUser)
            .WithMany()
            .HasForeignKey(cr => cr.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(cr => cr.ReviewedByUser)
            .WithMany()
            .HasForeignKey(cr => cr.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasIndex(cr => new { cr.PostId, cr.ReportedByUserId })
            .IsUnique()
            .HasDatabaseName("IX_ContentReports_PostId_ReportedByUserId");

        builder
            .HasIndex(cr => new { cr.Status, cr.CreatedAt })
            .HasDatabaseName("IX_ContentReports_Status_CreatedAt");

        builder.HasIndex(cr => cr.PostId).HasDatabaseName("IX_ContentReports_PostId");
    }
}
