using Lyke.Core.Enums;

namespace Lyke.Core.Entities;

public class Post : BaseEntity
{
    public Guid AuthorUserId { get; set; }
    public Guid? CreatorId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public MediaType MediaType { get; set; }
    public string? MediaUrls { get; set; } // JSON array
    public string? ThumbnailUrls { get; set; } // JSON array
    public PostStatus Status { get; set; }
    public string? ModerationNotes { get; set; }
    public DateTime? PublishedAt { get; set; }

    // Moderation tracking
    public Guid? ModeratedByUserId { get; set; }
    public DateTime? ModeratedAt { get; set; }

    // Navigation properties
    public User AuthorUser { get; set; } = null!;
    public Creator? Creator { get; set; }
    public User? ModeratedByUser { get; set; }
    public ICollection<PostProduct> PostProducts { get; set; } = new List<PostProduct>();
    public ICollection<Engagement> Engagements { get; set; } = new List<Engagement>();
    public ICollection<ClickEvent> ClickEvents { get; set; } = new List<ClickEvent>();
}
