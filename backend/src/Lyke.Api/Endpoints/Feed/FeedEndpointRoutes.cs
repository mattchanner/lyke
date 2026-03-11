using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;

namespace Lyke.Api.Endpoints.Feed;

public static class FeedEndpointRoutes
{
    public static IEndpointRouteBuilder MapFeedEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/feed/v1").WithTags("Feed");

        group
            .MapGet("/", GetFeed.Handle)
            .WithName("GetFeed")
            .WithSummary("Get personalized feed based on body profile")
            .Produces<ApiResponse<IReadOnlyList<FeedPostResponse>>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        group
            .MapGet("/explore", GetExploreFeed.Handle)
            .WithName("GetExploreFeed")
            .WithSummary("Get explore/discover feed (trending content)")
            .Produces<ApiResponse<IReadOnlyList<FeedPostResponse>>>(StatusCodes.Status200OK)
            .AllowAnonymous();

        return app;
    }
}
