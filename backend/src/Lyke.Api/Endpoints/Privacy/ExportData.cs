using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Privacy;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;

namespace Lyke.Api.Endpoints.Privacy;

public static class ExportData
{
    public static async Task<IResult> Handle(
        IPrivacyService privacyService,
        IAuditService auditService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var data = await privacyService.ExportUserDataAsync(userId, cancellationToken);

        await auditService.LogAsync(
            userId,
            AuditAction.DataExport,
            targetUserId: userId,
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString()
        );

        return Results.Ok(ApiResponse<DataExportResponse>.Ok(data));
    }
}
