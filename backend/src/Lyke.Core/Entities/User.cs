using Lyke.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace Lyke.Core.Entities;

public class User : IdentityUser<Guid>
{
    public UserType UserType { get; set; }
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Suspension tracking
    public DateTime? SuspendedAt { get; set; }
    public Guid? SuspendedByUserId { get; set; }
    public string? SuspensionReason { get; set; }

    // GDPR consent tracking
    public DateTime? PrivacyPolicyAcceptedAt { get; set; }
    public string? PrivacyPolicyVersion { get; set; }
    public bool MarketingOptIn { get; set; } = false;
    public string? ProfileImageUrl { get; set; }

    // Navigation properties
    public BodyProfile? BodyProfile { get; set; }
    public StyleProfile? StyleProfile { get; set; }
    public Creator? Creator { get; set; }
    public Retailer? Retailer { get; set; }
    public ICollection<Engagement> Engagements { get; set; } = new List<Engagement>();
    public ICollection<ClickEvent> ClickEvents { get; set; } = new List<ClickEvent>();
    public ICollection<Post> AuthoredPosts { get; set; } = new List<Post>();
    public ICollection<UserFollow> Following { get; set; } = new List<UserFollow>();
    public ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>();
}
