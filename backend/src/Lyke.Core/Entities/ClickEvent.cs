namespace Lyke.Core.Entities;

public class ClickEvent
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid PostId { get; set; }
    public Guid PostProductId { get; set; }
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public string? AttributionData { get; set; } // JSON

    // Navigation properties
    public User? User { get; set; }
    public Post Post { get; set; } = null!;
    public PostProduct PostProduct { get; set; } = null!;
    public CreatorEarning? Earning { get; set; }
}
