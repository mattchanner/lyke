using Lyke.Core.Enums;

namespace Lyke.Core.Entities;

public class ContentReport : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid ReportedByUserId { get; set; }
    public ReportReason Reason { get; set; }
    public string? AdditionalDetails { get; set; }
    public ReportStatus Status { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    // Navigation properties
    public Post Post { get; set; } = null!;
    public User ReportedByUser { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
}
