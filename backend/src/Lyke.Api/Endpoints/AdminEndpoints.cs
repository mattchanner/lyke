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

        // Content Reports
        group
            .MapGet("/reports", GetContentReportsAsync)
            .WithName("GetContentReports")
            .WithSummary("Query content reports")
            .Produces<ApiResponse<IReadOnlyList<ContentReportResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        group
            .MapGet("/reports/{reportId:guid}", GetContentReportAsync)
            .WithName("GetContentReport")
            .WithSummary("Get content report details")
            .Produces<ApiResponse<ContentReportResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPost("/reports/{reportId:guid}/review", ReviewContentReportAsync)
            .WithName("ReviewContentReport")
            .WithSummary("Review a content report")
            .Produces<ApiResponse<ContentReportResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Moderation Queue
        group
            .MapGet("/moderation-queue", GetModerationQueueAsync)
            .WithName("GetModerationQueue")
            .WithSummary("Get priority moderation queue")
            .Produces<ApiResponse<IReadOnlyList<ModerationQueueItemResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        // Bulk Actions
        group
            .MapPost("/posts/bulk-moderate", BulkModeratePostsAsync)
            .WithName("BulkModeratePosts")
            .WithSummary("Bulk moderate multiple posts")
            .Produces<ApiResponse<BulkActionResult>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        group
            .MapPost("/users/bulk-suspend", BulkSuspendUsersAsync)
            .WithName("BulkSuspendUsers")
            .WithSummary("Bulk suspend multiple users")
            .Produces<ApiResponse<BulkActionResult>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        // Audit Logs
        group
            .MapGet("/audit-logs", GetAuditLogsAsync)
            .WithName("GetAuditLogs")
            .WithSummary("Query audit trail")
            .Produces<ApiResponse<IReadOnlyList<AuditLogResponse>>>(StatusCodes.Status200OK)
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
        IAuditService auditService,
        HttpContext httpContext,
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

        await auditService.LogAsync(
            adminUserId.Value,
            AuditAction.AdminReviewVerification,
            entityType: "Creator",
            entityId: creatorId,
            details: new { request.Approve, request.RejectionReason },
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

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
        IAuditService auditService,
        HttpContext httpContext,
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

        await auditService.LogAsync(
            adminUserId.Value,
            AuditAction.AdminModeratePost,
            entityType: "Post",
            entityId: postId,
            details: new { request.Approve, request.RejectionReason },
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

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
        ClaimsPrincipal user,
        IAdminService adminService,
        IAuditService auditService,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var adminUserId = GetUserId(user);
        if (adminUserId == null)
            return Results.Unauthorized();

        var result = await adminService.GetUserAsync(userId, cancellationToken);

        await auditService.LogAsync(
            adminUserId.Value,
            AuditAction.AdminViewUserDetail,
            targetUserId: userId,
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

        return Results.Ok(ApiResponse<UserDetailResponse>.Ok(result));
    }

    private static async Task<IResult> SuspendUserAsync(
        Guid userId,
        [FromBody] SuspendUserRequest request,
        ClaimsPrincipal user,
        IAdminService adminService,
        IAuditService auditService,
        HttpContext httpContext,
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

        await auditService.LogAsync(
            adminUserId.Value,
            AuditAction.AdminSuspendUser,
            targetUserId: userId,
            details: new { request.Reason },
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

        return Results.Ok(ApiResponse<UserSuspensionResponse>.Ok(result));
    }

    private static async Task<IResult> UnsuspendUserAsync(
        Guid userId,
        ClaimsPrincipal user,
        IAdminService adminService,
        IAuditService auditService,
        HttpContext httpContext,
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

        await auditService.LogAsync(
            adminUserId.Value,
            AuditAction.AdminUnsuspendUser,
            targetUserId: userId,
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

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

    // Content Reports Handlers
    private static async Task<IResult> GetContentReportsAsync(
        [FromQuery] ReportStatus? status,
        [FromQuery] ReportReason? reason,
        [FromQuery] Guid? postId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        IAdminService adminService = default!,
        CancellationToken cancellationToken = default
    )
    {
        var request = new ContentReportQueryRequest(status, reason, postId, from, to, page, pageSize);
        var (reports, meta) = await adminService.GetContentReportsAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<ContentReportResponse>>.Ok(reports, meta));
    }

    private static async Task<IResult> GetContentReportAsync(
        Guid reportId,
        IAdminService adminService,
        CancellationToken cancellationToken
    )
    {
        var result = await adminService.GetContentReportAsync(reportId, cancellationToken);
        return Results.Ok(ApiResponse<ContentReportResponse>.Ok(result));
    }

    private static async Task<IResult> ReviewContentReportAsync(
        Guid reportId,
        [FromBody] ReviewContentReportRequest request,
        ClaimsPrincipal user,
        IAdminService adminService,
        IAuditService auditService,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var adminUserId = GetUserId(user);
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
            details: new { request.NewStatus, request.PostAction, request.ReviewNotes },
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

        return Results.Ok(ApiResponse<ContentReportResponse>.Ok(result));
    }

    // Moderation Queue Handler
    private static async Task<IResult> GetModerationQueueAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        IAdminService adminService = default!,
        CancellationToken cancellationToken = default
    )
    {
        var (items, meta) = await adminService.GetModerationQueueAsync(page, pageSize, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<ModerationQueueItemResponse>>.Ok(items, meta));
    }

    // Bulk Action Handlers
    private static async Task<IResult> BulkModeratePostsAsync(
        [FromBody] BulkModeratePostsRequest request,
        ClaimsPrincipal user,
        IAdminService adminService,
        IAuditService auditService,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var adminUserId = GetUserId(user);
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
            details: new { request.Action, PostCount = request.PostIds.Count, request.Reason },
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

        return Results.Ok(ApiResponse<BulkActionResult>.Ok(result));
    }

    private static async Task<IResult> BulkSuspendUsersAsync(
        [FromBody] BulkSuspendUsersRequest request,
        ClaimsPrincipal user,
        IAdminService adminService,
        IAuditService auditService,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var adminUserId = GetUserId(user);
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
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

        return Results.Ok(ApiResponse<BulkActionResult>.Ok(result));
    }

    // Audit Logs Handler
    private static async Task<IResult> GetAuditLogsAsync(
        [FromQuery] Guid? userId,
        [FromQuery] Guid? targetUserId,
        [FromQuery] AuditAction? action,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        IAuditService auditService = default!,
        CancellationToken cancellationToken = default
    )
    {
        var request = new AuditLogQueryRequest(userId, targetUserId, action, from, to, page, pageSize);
        var (logs, meta) = await auditService.GetAuditLogsAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<AuditLogResponse>>.Ok(logs, meta));
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
