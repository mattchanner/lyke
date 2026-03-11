using Lyke.Application.DTOs.Admin;
using Lyke.Core.Enums;

namespace Lyke.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        Guid userId,
        AuditAction action,
        Guid? targetUserId = null,
        string? entityType = null,
        Guid? entityId = null,
        object? details = null,
        string? ipAddress = null
    );

    Task<(IReadOnlyList<AuditLogResponse> Logs, DTOs.PaginationMeta Meta)> GetAuditLogsAsync(
        AuditLogQueryRequest request,
        CancellationToken cancellationToken = default
    );
}
