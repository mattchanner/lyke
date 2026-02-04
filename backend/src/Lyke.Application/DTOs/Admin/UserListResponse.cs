using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Admin;

public record UserListResponse(
    Guid Id,
    string? Email,
    string? UserName,
    UserType UserType,
    bool IsActive,
    bool EmailConfirmed,
    DateTime CreatedAt,
    DateTime? SuspendedAt,
    string? SuspensionReason
);
