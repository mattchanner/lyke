using Lyke.Core.Enums;

namespace Lyke.Core.Entities;

public class Engagement
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public EngagementType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}
