using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Admin;

public static class ModeratePost
{
    public static async Task<IResult> Handle(
        Guid postId,
        [FromBody] ModeratePostRequest request,
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

        var result = await adminService.ModeratePostAsync(
            adminUserId.Value,
            postId,
            request,
            cancellationToken
        );

        await auditService.LogAsync(
            adminUserId.Value,
            AuditAction.AdminModeratePost,
            entityType: "Post",
            entityId: postId,
            details: new { request.Approve, request.RejectionReason },
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString()
        );

        return Results.Ok(ApiResponse<PostModerationResponse>.Ok(result));
    }
}
