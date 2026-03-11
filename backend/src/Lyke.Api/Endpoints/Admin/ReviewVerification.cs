using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Admin;

public static class ReviewVerification
{
    public static async Task<IResult> Handle(
        Guid creatorId,
        [FromBody] ReviewVerificationRequest request,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        IAuditService auditService,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var adminUserId = UserIdExtractor.GetUserId(user);
        if (adminUserId == null)
            return Results.Unauthorized();

        var result = await creatorService.ReviewVerificationAsync(
            adminUserId.Value,
            creatorId,
            request,
            cancellationToken
        );

        await auditService.LogAsync(
            adminUserId.Value,
            AuditAction.AdminReviewVerification,
            entityType: "Creator",
            entityId: creatorId,
            details: new { request.Approve, request.RejectionReason },
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString()
        );

        return Results.Ok(ApiResponse<VerificationStatusResponse>.Ok(result));
    }
}
