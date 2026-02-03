using Lyke.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace Lyke.Core.Entities;

public class User : IdentityUser<Guid>
{
    public UserType UserType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public BodyProfile? BodyProfile { get; set; }
    public Creator? Creator { get; set; }
    public ICollection<Engagement> Engagements { get; set; } = new List<Engagement>();
    public ICollection<ClickEvent> ClickEvents { get; set; } = new List<ClickEvent>();
}
