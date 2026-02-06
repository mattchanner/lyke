using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/v1")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        // Verification Management
        group
            .MapGet("/verifications", GetPendingVerificationsAsync)
            .WithName("GetPendingVerifications")
            .WithSummary("Get creator verification requests")
            .Produces<ApiResponse<IReadOnlyList<PendingVerificationResponse>>>(
                StatusCodes.Status200OK
            )
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        group
            .MapGet("/verifications/{creatorId:guid}", GetVerificationDetailsAsync)
            .WithName("GetVerificationDetails")
            .WithSummary("Get detailed verification request for a creator")
            .Produces<ApiResponse<PendingVerificationResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPost("/verifications/{creatorId:guid}/review", ReviewVerificationAsync)
            .WithName("ReviewVerification")
            .WithSummary("Approve or reject a creator verification request")
            .Produces<ApiResponse<VerificationStatusResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Post Moderation
        group
            .MapGet("/posts", GetPendingPostsAsync)
            .WithName("GetPendingPosts")
            .WithSummary("Get posts pending review")
            .Produces<ApiResponse<IReadOnlyList<PendingPostResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        group
            .MapGet("/posts/{postId:guid}", GetPostForModerationAsync)
            .WithName("GetPostForModeration")
            .WithSummary("Get post details for moderation")
            .Produces<ApiResponse<PendingPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPost("/posts/{postId:guid}/moderate", ModeratePostAsync)
            .WithName("ModeratePost")
            .WithSummary("Approve or reject a post")
            .Produces<ApiResponse<PostModerationResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // User Management
        group
            .MapGet("/users", GetUsersAsync)
            .WithName("GetUsers")
            .WithSummary("Get users with optional filtering")
            .Produces<ApiResponse<IReadOnlyList<UserListResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        group
            .MapGet("/users/{userId:guid}", GetUserAsync)
            .WithName("GetUserDetail")
            .WithSummary("Get detailed user information")
            .Produces<ApiResponse<UserDetailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPost("/users/{userId:guid}/suspend", SuspendUserAsync)
            .WithName("SuspendUser")
            .WithSummary("Suspend a user account")
            .Produces<ApiResponse<UserSuspensionResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPost("/users/{userId:guid}/unsuspend", UnsuspendUserAsync)
            .WithName("UnsuspendUser")
            .WithSummary("Unsuspend a user account")
            .Produces<ApiResponse<UserSuspensionResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Platform Stats
        group
            .MapGet("/stats", GetPlatformStatsAsync)
            .WithName("GetPlatformStats")
            .WithSummary("Get platform statistics")
            .Produces<ApiResponse<PlatformStatsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> GetPendingVerificationsAsync(
        [FromQuery] VerificationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        ICreatorService creatorService = default!,
        CancellationToken cancellationToken = default
    )
    {
        var (verifications, meta) = await creatorService.GetPendingVerificationsAsync(
            status,
            page,
            pageSize,
            cancellationToken
        );
        return Results.Ok(
            ApiResponse<IReadOnlyList<PendingVerificationResponse>>.Ok(verifications, meta)
        );
    }

    private static async Task<IResult> GetVerificationDetailsAsync(
        Guid creatorId,
        ICreatorService creatorService,
        CancellationToken cancellationToken
    )
    {
        var result = await creatorService.GetVerificationDetailsAsync(creatorId, cancellationToken);
        return Results.Ok(ApiResponse<PendingVerificationResponse>.Ok(result));
    }

    private static async Task<IResult> ReviewVerificationAsync(
        Guid creatorId,
        [FromBody] ReviewVerificationRequest request,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken
    )
    {
        var adminUserId = GetUserId(user);
        if (adminUserId == null)
            return Results.Unauthorized();

        var result = await creatorService.ReviewVerificationAsync(
            adminUserId.Value,
            creatorId,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<VerificationStatusResponse>.Ok(result));
    }

    // Post Moderation Handlers
    private static async Task<IResult> GetPendingPostsAsync(
        [FromQuery] PostStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        IAdminService adminService = default!,
        CancellationToken cancellationToken = default
    )
    {
        var (posts, meta) = await adminService.GetPendingPostsAsync(
            status,
            page,
            pageSize,
            cancellationToken
        );
        return Results.Ok(ApiResponse<IReadOnlyList<PendingPostResponse>>.Ok(posts, meta));
    }

    private static async Task<IResult> GetPostForModerationAsync(
        Guid postId,
        IAdminService adminService,
        CancellationToken cancellationToken
    )
    {
        var result = await adminService.GetPostForModerationAsync(postId, cancellationToken);
        return Results.Ok(ApiResponse<PendingPostResponse>.Ok(result));
    }

    private static async Task<IResult> ModeratePostAsync(
        Guid postId,
        [FromBody] ModeratePostRequest request,
        ClaimsPrincipal user,
        IAdminService adminService,
        CancellationToken cancellationToken
    )
    {
        var adminUserId = GetUserId(user);
        if (adminUserId == null)
            return Results.Unauthorized();

        var result = await adminService.ModeratePostAsync(
            adminUserId.Value,
            postId,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<PostModerationResponse>.Ok(result));
    }

    // User Management Handlers
    private static async Task<IResult> GetUsersAsync(
        [FromQuery] UserType? userType,
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        IAdminService adminService = default!,
        CancellationToken cancellationToken = default
    )
    {
        var request = new UserListRequest(userType, isActive, search, page, pageSize);
        var (users, meta) = await adminService.GetUsersAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<UserListResponse>>.Ok(users, meta));
    }

    private static async Task<IResult> GetUserAsync(
        Guid userId,
        IAdminService adminService,
        CancellationToken cancellationToken
    )
    {
        var result = await adminService.GetUserAsync(userId, cancellationToken);
        return Results.Ok(ApiResponse<UserDetailResponse>.Ok(result));
    }

    private static async Task<IResult> SuspendUserAsync(
        Guid userId,
        [FromBody] SuspendUserRequest request,
        ClaimsPrincipal user,
        IAdminService adminService,
        CancellationToken cancellationToken
    )
    {
        var adminUserId = GetUserId(user);
        if (adminUserId == null)
            return Results.Unauthorized();

        var result = await adminService.SuspendUserAsync(
            adminUserId.Value,
            userId,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<UserSuspensionResponse>.Ok(result));
    }

    private static async Task<IResult> UnsuspendUserAsync(
        Guid userId,
        ClaimsPrincipal user,
        IAdminService adminService,
        CancellationToken cancellationToken
    )
    {
        var adminUserId = GetUserId(user);
        if (adminUserId == null)
            return Results.Unauthorized();

        var result = await adminService.UnsuspendUserAsync(
            adminUserId.Value,
            userId,
            cancellationToken
        );
        return Results.Ok(ApiResponse<UserSuspensionResponse>.Ok(result));
    }

    // Platform Stats Handler
    private static async Task<IResult> GetPlatformStatsAsync(
        IAdminService adminService,
        CancellationToken cancellationToken
    )
    {
        var result = await adminService.GetPlatformStatsAsync(cancellationToken);
        return Results.Ok(ApiResponse<PlatformStatsResponse>.Ok(result));
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim =
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
