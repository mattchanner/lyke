using Lyke.Application.DTOs.Profile;
using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Feed;

public record AuthorSummaryResponse(
    Guid UserId,
    Guid? CreatorId,
    string DisplayName,
    bool IsVerified,
    UserType UserType,
    AnonymizedBodyProfileResponse? BodyProfile,
    string? ProfileImageUrl
);

public record AuthorDetailResponse(
    Guid UserId,
    Guid? CreatorId,
    string DisplayName,
    string? Bio,
    bool IsVerified,
    UserType UserType,
    AnonymizedBodyProfileResponse? BodyProfile,
    int TotalPosts,
    string? ProfileImageUrl
);
