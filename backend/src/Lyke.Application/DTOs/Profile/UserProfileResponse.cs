using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Profile;

public record UserProfileResponse(
    Guid Id,
    string Email,
    UserType UserType,
    bool HasBodyProfile,
    int ProfileCompleteness,
    DateTime CreatedAt
);
