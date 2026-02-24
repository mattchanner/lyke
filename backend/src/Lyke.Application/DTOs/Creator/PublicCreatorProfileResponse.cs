using Lyke.Application.DTOs.Profile;

namespace Lyke.Application.DTOs.Creator;

public record PublicCreatorProfileResponse(
    Guid Id,
    string DisplayName,
    string? Bio,
    bool IsVerified,
    AnonymizedBodyProfileResponse? BodyProfile,
    string? ProfileImageUrl,
    int PublishedPosts,
    DateTime CreatedAt
);
