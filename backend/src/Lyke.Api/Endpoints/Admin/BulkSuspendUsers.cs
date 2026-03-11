using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Admin;

public static class BulkSuspendUsers
{
    public static async Task<IResult> Handle(
        [FromBody] BulkSuspendUsersRequest request,
        ClaimsPrincipal user,
        IAdminService adminService,
        IAuditService auditService,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var adminUserId = UserIdExtractor.GetUserId(user);
        if (adminUserId == null)
            return Results.Unauthorized();

        var result = await adminService.BulkSuspendUsersAsync(
            adminUserId.Value,
            request,
            cancellationToken
        );

        await auditService.LogAsync(
            adminUserId.Value,
            AuditAction.AdminBulkSuspendUser,
            details: new { UserCount = request.UserIds.Count, request.Reason },
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString()
        );

        return Results.Ok(ApiResponse<BulkActionResult>.Ok(result));
    }
}
