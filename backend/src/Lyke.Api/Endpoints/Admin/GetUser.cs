using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;

namespace Lyke.Api.Endpoints.Admin;

public static class GetUser
{
    public static async Task<IResult> Handle(
        Guid userId,
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

        var result = await adminService.GetUserAsync(userId, cancellationToken);

        await auditService.LogAsync(
            adminUserId.Value,
            AuditAction.AdminViewUserDetail,
            targetUserId: userId,
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString()
        );

        return Results.Ok(ApiResponse<UserDetailResponse>.Ok(result));
    }
}
