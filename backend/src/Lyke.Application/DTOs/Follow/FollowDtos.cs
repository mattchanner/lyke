using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Follow;

public record FollowedCreatorResponse(
    Guid CreatorId,
    string DisplayName,
    bool IsVerified,
    string? ProfileImageUrl,
    DateTime FollowedAt
);

public record FollowedUserResponse(
    Guid UserId,
    string DisplayName,
    UserType UserType,
    bool IsVerified,
    Guid? CreatorId,
    string? ProfileImageUrl,
    DateTime FollowedAt
);
