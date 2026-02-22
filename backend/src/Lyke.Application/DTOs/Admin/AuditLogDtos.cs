using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Admin;

public record AuditLogResponse(
    Guid Id,
    Guid UserId,
    Guid? TargetUserId,
    AuditAction Action,
    string? EntityType,
    Guid? EntityId,
    string? Details,
    string? IpAddress,
    DateTime Timestamp);

public record AuditLogQueryRequest(
    Guid? UserId = null,
    Guid? TargetUserId = null,
    AuditAction? Action = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 20);
