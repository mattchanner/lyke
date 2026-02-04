namespace Lyke.Application.DTOs.Creator;

public record CreatorProfileResponse(
    Guid Id,
    string DisplayName,
    string? Bio,
    bool IsVerified,
    Dictionary<string, string>? SocialLinks,
    int TotalPosts,
    int PublishedPosts,
    int DraftPosts,
    int PendingReviewPosts,
    DateTime CreatedAt
);
