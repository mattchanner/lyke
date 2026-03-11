using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Admin;

public static class ReviewContentReport
{
    public static async Task<IResult> Handle(
        Guid reportId,
        [FromBody] ReviewContentReportRequest request,
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

        var result = await adminService.ReviewContentReportAsync(
            adminUserId.Value,
            reportId,
            request,
            cancellationToken
        );

        await auditService.LogAsync(
            adminUserId.Value,
            AuditAction.AdminReviewContentReport,
            entityType: "ContentReport",
            entityId: reportId,
            details: new
            {
                request.NewStatus,
                request.PostAction,
                request.ReviewNotes,
            },
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString()
        );

        return Results.Ok(ApiResponse<ContentReportResponse>.Ok(result));
    }
}
