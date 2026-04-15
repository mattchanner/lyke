using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Profile;

public record PublicUserProfileResponse(
    Guid UserId,
    string DisplayName,
    string? Bio,
    bool IsVerified,
    UserType UserType,
    Guid? CreatorId,
    AnonymizedBodyProfileResponse? BodyProfile,
    string? ProfileImageUrl,
    int PublishedPostCount,
    int FollowerCount,
    DateTime JoinedAt
);
