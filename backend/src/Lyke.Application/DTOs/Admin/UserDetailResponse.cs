using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Admin;

public record UserDetailResponse(
    Guid Id,
    string? Email,
    string? UserName,
    UserType UserType,
    bool IsActive,
    bool EmailConfirmed,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? SuspendedAt,
    Guid? SuspendedByUserId,
    string? SuspensionReason,
    bool HasBodyProfile,
    bool IsCreator,
    CreatorInfo? CreatorInfo
);

public record CreatorInfo(
    Guid CreatorId,
    string DisplayName,
    bool IsVerified,
    VerificationStatus VerificationStatus,
    int TotalPosts,
    int PublishedPosts
);
