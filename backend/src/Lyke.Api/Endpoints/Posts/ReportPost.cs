using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Posts;

public static class ReportPost
{
    public static async Task<IResult> Handle(
        Guid id,
        [FromBody] CreateContentReportRequest request,
        ClaimsPrincipal user,
        IFeedService feedService,
        IAuditService auditService,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        await feedService.ReportPostAsync(id, userId.Value, request, cancellationToken);

        await auditService.LogAsync(
            userId.Value,
            AuditAction.UserReportContent,
            entityType: "Post",
            entityId: id,
            details: new { request.Reason, request.AdditionalDetails },
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString()
        );

        return Results.NoContent();
    }
}
