using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.DTOs.Creator;

namespace Lyke.Api.Endpoints.Admin;

public static class AdminEndpointRoutes
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/v1")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        // Verification Management
        group
            .MapGet("/verifications", GetPendingVerifications.Handle)
            .WithName("GetPendingVerifications")
            .WithSummary("Get creator verification requests")
            .Produces<ApiResponse<IReadOnlyList<PendingVerificationResponse>>>(
                StatusCodes.Status200OK
            )
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        group
            .MapGet("/verifications/{creatorId:guid}", GetVerificationDetails.Handle)
            .WithName("GetVerificationDetails")
            .WithSummary("Get detailed verification request for a creator")
            .Produces<ApiResponse<PendingVerificationResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPost("/verifications/{creatorId:guid}/review", ReviewVerification.Handle)
            .WithName("ReviewVerification")
            .WithSummary("Approve or reject a creator verification request")
            .Produces<ApiResponse<VerificationStatusResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Post Moderation
        group
            .MapGet("/posts", GetPendingPosts.Handle)
            .WithName("GetPendingPosts")
            .WithSummary("Get posts pending review")
            .Produces<ApiResponse<IReadOnlyList<PendingPostResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        group
            .MapGet("/posts/{postId:guid}", GetPostForModeration.Handle)
            .WithName("GetPostForModeration")
            .WithSummary("Get post details for moderation")
            .Produces<ApiResponse<PendingPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPost("/posts/{postId:guid}/moderate", ModeratePost.Handle)
            .WithName("ModeratePost")
            .WithSummary("Approve or reject a post")
            .Produces<ApiResponse<PostModerationResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // User Management
        group
            .MapGet("/users", GetUsers.Handle)
            .WithName("GetUsers")
            .WithSummary("Get users with optional filtering")
            .Produces<ApiResponse<IReadOnlyList<UserListResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        group
            .MapGet("/users/{userId:guid}", GetUser.Handle)
            .WithName("GetUserDetail")
            .WithSummary("Get detailed user information")
            .Produces<ApiResponse<UserDetailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPost("/users/{userId:guid}/suspend", SuspendUser.Handle)
            .WithName("SuspendUser")
            .WithSummary("Suspend a user account")
            .Produces<ApiResponse<UserSuspensionResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPost("/users/{userId:guid}/unsuspend", UnsuspendUser.Handle)
            .WithName("UnsuspendUser")
            .WithSummary("Unsuspend a user account")
            .Produces<ApiResponse<UserSuspensionResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Platform Stats
        group
            .MapGet("/stats", GetPlatformStats.Handle)
            .WithName("GetPlatformStats")
            .WithSummary("Get platform statistics")
            .Produces<ApiResponse<PlatformStatsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        group
            .MapGet("/analytics", GetPlatformAnalytics.Handle)
            .WithName("GetPlatformAnalytics")
            .WithSummary("Get platform analytics with daily metrics")
            .Produces<ApiResponse<AdminAnalyticsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        // Content Reports
        group
            .MapGet("/reports", GetContentReports.Handle)
            .WithName("GetContentReports")
            .WithSummary("Query content reports")
            .Produces<ApiResponse<IReadOnlyList<ContentReportResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        group
            .MapGet("/reports/{reportId:guid}", GetContentReport.Handle)
            .WithName("GetContentReport")
            .WithSummary("Get content report details")
            .Produces<ApiResponse<ContentReportResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPost("/reports/{reportId:guid}/review", ReviewContentReport.Handle)
            .WithName("ReviewContentReport")
            .WithSummary("Review a content report")
            .Produces<ApiResponse<ContentReportResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Moderation Queue
        group
            .MapGet("/moderation-queue", GetModerationQueue.Handle)
            .WithName("GetModerationQueue")
            .WithSummary("Get priority moderation queue")
            .Produces<ApiResponse<IReadOnlyList<ModerationQueueItemResponse>>>(
                StatusCodes.Status200OK
            )
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        // Bulk Actions
        group
            .MapPost("/posts/bulk-moderate", BulkModeratePosts.Handle)
            .WithName("BulkModeratePosts")
            .WithSummary("Bulk moderate multiple posts")
            .Produces<ApiResponse<BulkActionResult>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        group
            .MapPost("/users/bulk-suspend", BulkSuspendUsers.Handle)
            .WithName("BulkSuspendUsers")
            .WithSummary("Bulk suspend multiple users")
            .Produces<ApiResponse<BulkActionResult>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        // Audit Logs
        group
            .MapGet("/audit-logs", GetAuditLogs.Handle)
            .WithName("GetAuditLogs")
            .WithSummary("Query audit trail")
            .Produces<ApiResponse<IReadOnlyList<AuditLogResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        return app;
    }
}
