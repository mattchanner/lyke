using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints;

public static class FeedEndpoints
{
    public static IEndpointRouteBuilder MapFeedEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/feed/v1")
            .WithTags("Feed");

        group.MapGet("/", GetFeedAsync)
            .WithName("GetFeed")
            .WithSummary("Get personalized feed based on body profile")
            .Produces<ApiResponse<IReadOnlyList<FeedPostResponse>>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        group.MapGet("/explore", GetExploreFeedAsync)
            .WithName("GetExploreFeed")
            .WithSummary("Get explore/discover feed (trending content)")
            .Produces<ApiResponse<IReadOnlyList<FeedPostResponse>>>(StatusCodes.Status200OK)
            .AllowAnonymous();

        return app;
    }

    public static IEndpointRouteBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts/v1")
            .WithTags("Posts");

        group.MapGet("/{id:guid}", GetPostAsync)
            .WithName("GetPost")
            .WithSummary("Get post details by ID")
            .Produces<ApiResponse<PostDetailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapGet("/{id:guid}/similar", GetSimilarPostsAsync)
            .WithName("GetSimilarPosts")
            .WithSummary("Get posts similar to the specified post")
            .Produces<ApiResponse<IReadOnlyList<FeedPostResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapPost("/{id:guid}/engage", EngageAsync)
            .WithName("EngagePost")
            .WithSummary("Record engagement (view, like, save, share)")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapDelete("/{id:guid}/engage", RemoveEngagementAsync)
            .WithName("RemoveEngagement")
            .WithSummary("Remove engagement (unlike, unsave)")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .RequireAuthorization();

        group.MapGet("/saved", GetSavedPostsAsync)
            .WithName("GetSavedPosts")
            .WithSummary("Get user's saved posts")
            .Produces<ApiResponse<IReadOnlyList<FeedPostResponse>>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        return app;
    }

    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/search/v1")
            .WithTags("Search");

        group.MapGet("/", SearchAsync)
            .WithName("Search")
            .WithSummary("Search posts, products, and creators")
            .Produces<ApiResponse<SearchResponse>>(StatusCodes.Status200OK)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> GetFeedAsync(
        [AsParameters] FeedQueryParams queryParams,
        ClaimsPrincipal user,
        IFeedService feedService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var request = new FeedRequest(
            queryParams.Page ?? 1,
            queryParams.PageSize ?? 20,
            queryParams.Category,
            queryParams.RetailerId,
            queryParams.FitTagIds?.Split(',').Select(int.Parse).ToList(),
            queryParams.SortBy ?? FeedSortBy.Relevance
        );

        var (posts, meta) = await feedService.GetFeedAsync(userId.Value, request, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<FeedPostResponse>>.Ok(posts, meta));
    }

    private static async Task<IResult> GetExploreFeedAsync(
        [AsParameters] FeedQueryParams queryParams,
        ClaimsPrincipal user,
        IFeedService feedService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);

        var request = new FeedRequest(
            queryParams.Page ?? 1,
            queryParams.PageSize ?? 20,
            queryParams.Category,
            queryParams.RetailerId,
            queryParams.FitTagIds?.Split(',').Select(int.Parse).ToList(),
            queryParams.SortBy ?? FeedSortBy.Recent
        );

        var (posts, meta) = await feedService.GetExploreFeedAsync(userId, request, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<FeedPostResponse>>.Ok(posts, meta));
    }

    private static async Task<IResult> GetPostAsync(
        Guid id,
        ClaimsPrincipal user,
        IFeedService feedService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        var post = await feedService.GetPostAsync(id, userId, cancellationToken);
        return Results.Ok(ApiResponse<PostDetailResponse>.Ok(post));
    }

    private static async Task<IResult> GetSimilarPostsAsync(
        Guid id,
        [FromQuery] int? limit,
        ClaimsPrincipal user,
        IFeedService feedService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        var posts = await feedService.GetSimilarPostsAsync(id, userId, limit ?? 10, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<FeedPostResponse>>.Ok(posts));
    }

    private static async Task<IResult> EngageAsync(
        Guid id,
        [FromBody] EngageRequest request,
        ClaimsPrincipal user,
        IFeedService feedService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        await feedService.EngageAsync(id, userId.Value, request, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }

    private static async Task<IResult> RemoveEngagementAsync(
        Guid id,
        [FromBody] EngageRequest request,
        ClaimsPrincipal user,
        IFeedService feedService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        await feedService.RemoveEngagementAsync(id, userId.Value, request, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }

    private static async Task<IResult> GetSavedPostsAsync(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        ClaimsPrincipal user,
        IFeedService feedService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var (posts, meta) = await feedService.GetSavedPostsAsync(
            userId.Value,
            page ?? 1,
            pageSize ?? 20,
            cancellationToken);

        return Results.Ok(ApiResponse<IReadOnlyList<FeedPostResponse>>.Ok(posts, meta));
    }

    private static async Task<IResult> SearchAsync(
        [AsParameters] SearchQueryParams queryParams,
        ClaimsPrincipal user,
        IFeedService feedService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);

        var request = new SearchRequest(
            queryParams.Q ?? "",
            queryParams.Page ?? 1,
            queryParams.PageSize ?? 20,
            queryParams.Type
        );

        var result = await feedService.SearchAsync(request, userId, cancellationToken);
        return Results.Ok(ApiResponse<SearchResponse>.Ok(result));
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

public class FeedQueryParams
{
    [FromQuery(Name = "page")]
    public int? Page { get; set; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }

    [FromQuery(Name = "category")]
    public string? Category { get; set; }

    [FromQuery(Name = "retailerId")]
    public Guid? RetailerId { get; set; }

    [FromQuery(Name = "fitTagIds")]
    public string? FitTagIds { get; set; }

    [FromQuery(Name = "sortBy")]
    public FeedSortBy? SortBy { get; set; }
}

public class SearchQueryParams
{
    [FromQuery(Name = "q")]
    public string? Q { get; set; }

    [FromQuery(Name = "page")]
    public int? Page { get; set; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }

    [FromQuery(Name = "type")]
    public SearchType? Type { get; set; }
}
