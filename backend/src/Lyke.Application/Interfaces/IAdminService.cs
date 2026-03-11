using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Core.Enums;

namespace Lyke.Application.Interfaces;

public interface IAdminService
{
    // Post Moderation
    Task<(IReadOnlyList<PendingPostResponse> Posts, PaginationMeta Meta)> GetPendingPostsAsync(
        PostStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<PendingPostResponse> GetPostForModerationAsync(
        Guid postId,
        CancellationToken cancellationToken = default
    );

    Task<PostModerationResponse> ModeratePostAsync(
        Guid adminUserId,
        Guid postId,
        ModeratePostRequest request,
        CancellationToken cancellationToken = default
    );

    // User Management
    Task<(IReadOnlyList<UserListResponse> Users, PaginationMeta Meta)> GetUsersAsync(
        UserListRequest request,
        CancellationToken cancellationToken = default
    );

    Task<UserDetailResponse> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<UserSuspensionResponse> SuspendUserAsync(
        Guid adminUserId,
        Guid userId,
        SuspendUserRequest request,
        CancellationToken cancellationToken = default
    );

    Task<UserSuspensionResponse> UnsuspendUserAsync(
        Guid adminUserId,
        Guid userId,
        CancellationToken cancellationToken = default
    );

    // Content Reports
    Task<(
        IReadOnlyList<ContentReportResponse> Reports,
        PaginationMeta Meta
    )> GetContentReportsAsync(
        ContentReportQueryRequest request,
        CancellationToken cancellationToken = default
    );

    Task<ContentReportResponse> GetContentReportAsync(
        Guid reportId,
        CancellationToken cancellationToken = default
    );

    Task<ContentReportResponse> ReviewContentReportAsync(
        Guid adminUserId,
        Guid reportId,
        ReviewContentReportRequest request,
        CancellationToken cancellationToken = default
    );

    // Moderation Queue
    Task<(
        IReadOnlyList<ModerationQueueItemResponse> Items,
        PaginationMeta Meta
    )> GetModerationQueueAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    // Bulk Actions
    Task<BulkActionResult> BulkModeratePostsAsync(
        Guid adminUserId,
        BulkModeratePostsRequest request,
        CancellationToken cancellationToken = default
    );

    Task<BulkActionResult> BulkSuspendUsersAsync(
        Guid adminUserId,
        BulkSuspendUsersRequest request,
        CancellationToken cancellationToken = default
    );

    // Platform Stats
    Task<PlatformStatsResponse> GetPlatformStatsAsync(
        CancellationToken cancellationToken = default
    );

    Task<AdminAnalyticsResponse> GetPlatformAnalyticsAsync(
        AdminAnalyticsRequest request,
        CancellationToken cancellationToken = default
    );
}
