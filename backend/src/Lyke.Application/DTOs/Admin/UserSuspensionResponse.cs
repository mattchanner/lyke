namespace Lyke.Application.DTOs.Admin;

public record UserSuspensionResponse(
    Guid UserId,
    bool IsActive,
    DateTime? SuspendedAt,
    Guid? SuspendedByUserId,
    string? SuspensionReason
);
