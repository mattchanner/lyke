using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        // Verification Management
        group.MapGet("/verifications", GetPendingVerificationsAsync)
            .WithName("GetPendingVerifications")
            .WithSummary("Get creator verification requests")
            .Produces<ApiResponse<IReadOnlyList<PendingVerificationResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        group.MapGet("/verifications/{creatorId:guid}", GetVerificationDetailsAsync)
            .WithName("GetVerificationDetails")
            .WithSummary("Get detailed verification request for a creator")
            .Produces<ApiResponse<PendingVerificationResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/verifications/{creatorId:guid}/review", ReviewVerificationAsync)
            .WithName("ReviewVerification")
            .WithSummary("Approve or reject a creator verification request")
            .Produces<ApiResponse<VerificationStatusResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetPendingVerificationsAsync(
        [FromQuery] VerificationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        ICreatorService creatorService = default!,
        CancellationToken cancellationToken = default)
    {
        var (verifications, meta) = await creatorService.GetPendingVerificationsAsync(
            status, page, pageSize, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<PendingVerificationResponse>>.Ok(verifications, meta));
    }

    private static async Task<IResult> GetVerificationDetailsAsync(
        Guid creatorId,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var result = await creatorService.GetVerificationDetailsAsync(creatorId, cancellationToken);
        return Results.Ok(ApiResponse<PendingVerificationResponse>.Ok(result));
    }

    private static async Task<IResult> ReviewVerificationAsync(
        Guid creatorId,
        [FromBody] ReviewVerificationRequest request,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetUserId(user);
        if (adminUserId == null) return Results.Unauthorized();

        var result = await creatorService.ReviewVerificationAsync(
            adminUserId.Value, creatorId, request, cancellationToken);
        return Results.Ok(ApiResponse<VerificationStatusResponse>.Ok(result));
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
