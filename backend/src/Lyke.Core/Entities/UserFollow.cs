namespace Lyke.Core.Entities;

public class UserFollow
{
    public Guid Id { get; set; }
    public Guid FollowerUserId { get; set; }
    public Guid FollowedUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Follower { get; set; } = null!;
    public User Followed { get; set; } = null!;
}
