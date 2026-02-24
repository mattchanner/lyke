namespace Lyke.Application.DTOs.Follow;

public record FollowedCreatorResponse(
    Guid CreatorId,
    string DisplayName,
    bool IsVerified,
    string? ProfileImageUrl,
    DateTime FollowedAt
);
