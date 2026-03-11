using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;

namespace Lyke.Api.Endpoints.Posts;

public static class PostEndpointRoutes
{
    public static IEndpointRouteBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts/v1").WithTags("Posts");

        group
            .MapGet("/{id:guid}", GetPost.Handle)
            .WithName("GetPost")
            .WithSummary("Get post details by ID")
            .Produces<ApiResponse<PostDetailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group
            .MapGet("/{id:guid}/similar", GetSimilarPosts.Handle)
            .WithName("GetSimilarPosts")
            .WithSummary("Get posts similar to the specified post")
            .Produces<ApiResponse<IReadOnlyList<FeedPostResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group
            .MapPost("/{id:guid}/engage", Engage.Handle)
            .WithName("EngagePost")
            .WithSummary("Record engagement (view, like, save, share)")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group
            .MapDelete("/{id:guid}/engage", RemoveEngagement.Handle)
            .WithName("RemoveEngagement")
            .WithSummary("Remove engagement (unlike, unsave)")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .RequireAuthorization();

        group
            .MapGet("/saved", GetSavedPosts.Handle)
            .WithName("GetSavedPosts")
            .WithSummary("Get user's saved posts")
            .Produces<ApiResponse<IReadOnlyList<FeedPostResponse>>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        group
            .MapGet("/liked", GetLikedPosts.Handle)
            .WithName("GetLikedPosts")
            .WithSummary("Get user's liked posts")
            .Produces<ApiResponse<IReadOnlyList<FeedPostResponse>>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        group
            .MapPost("/{id:guid}/report", ReportPost.Handle)
            .WithName("ReportPost")
            .WithSummary("Report a post for content violation")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return app;
    }
}
