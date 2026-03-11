using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Admin;

public static class BulkModeratePosts
{
    public static async Task<IResult> Handle(
        [FromBody] BulkModeratePostsRequest request,
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

        var result = await adminService.BulkModeratePostsAsync(
            adminUserId.Value,
            request,
            cancellationToken
        );

        await auditService.LogAsync(
            adminUserId.Value,
            AuditAction.AdminBulkModeratePost,
            details: new
            {
                request.Action,
                PostCount = request.PostIds.Count,
                request.Reason,
            },
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString()
        );

        return Results.Ok(ApiResponse<BulkActionResult>.Ok(result));
    }
}
