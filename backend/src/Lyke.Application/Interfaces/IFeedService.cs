using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;

namespace Lyke.Application.Interfaces;

public interface IFeedService
{
    /// <summary>
    /// Get personalized feed based on user's body profile
    /// </summary>
    Task<(IReadOnlyList<FeedPostResponse> Posts, PaginationMeta Meta)> GetFeedAsync(
        Guid userId,
        FeedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get explore/discover feed (not personalized, shows trending content)
    /// </summary>
    Task<(IReadOnlyList<FeedPostResponse> Posts, PaginationMeta Meta)> GetExploreFeedAsync(
        Guid? userId,
        FeedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed post by ID
    /// </summary>
    Task<PostDetailResponse> GetPostAsync(
        Guid postId,
        Guid? userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get posts similar to the specified post
    /// </summary>
    Task<IReadOnlyList<FeedPostResponse>> GetSimilarPostsAsync(
        Guid postId,
        Guid? userId,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Record user engagement (view, like, save, share)
    /// </summary>
    Task EngageAsync(
        Guid postId,
        Guid userId,
        EngageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove engagement (unlike, unsave)
    /// </summary>
    Task RemoveEngagementAsync(
        Guid postId,
        Guid userId,
        EngageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's saved posts
    /// </summary>
    Task<(IReadOnlyList<FeedPostResponse> Posts, PaginationMeta Meta)> GetSavedPostsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search posts, products, and creators
    /// </summary>
    Task<SearchResponse> SearchAsync(
        SearchRequest request,
        Guid? userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Report a post for content violation
    /// </summary>
    Task ReportPostAsync(
        Guid postId,
        Guid userId,
        CreateContentReportRequest request,
        CancellationToken cancellationToken = default);
}
