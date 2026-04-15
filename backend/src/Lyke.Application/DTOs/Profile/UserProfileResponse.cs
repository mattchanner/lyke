using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Profile;

public record UserProfileResponse(
    Guid Id,
    string Email,
    string? DisplayName,
    UserType UserType,
    bool HasBodyProfile,
    int ProfileCompleteness,
    DateTime CreatedAt,
    string? ProfileImageUrl,
    bool IsEmailVerified
);
