namespace Lyke.Core.Entities;

public class Creator : BaseEntity
{
    public Guid UserId { get; set; }
    public required string DisplayName { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; }
    public string? SocialLinks { get; set; } // JSON
    public string? PayoutDetails { get; set; } // Encrypted

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<CreatorEarning> Earnings { get; set; } = new List<CreatorEarning>();
}
