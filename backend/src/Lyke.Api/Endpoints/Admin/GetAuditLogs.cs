using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Admin;

public static class GetAuditLogs
{
    public static async Task<IResult> Handle(
        [FromQuery] Guid? userId,
        [FromQuery] Guid? targetUserId,
        [FromQuery] AuditAction? action,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        IAuditService auditService = default!,
        CancellationToken cancellationToken = default
    )
    {
        var request = new AuditLogQueryRequest(
            userId,
            targetUserId,
            action,
            from,
            to,
            page,
            pageSize
        );
        var (logs, meta) = await auditService.GetAuditLogsAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<AuditLogResponse>>.Ok(logs, meta));
    }
}
