using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Posts;

public static class GetLikedPosts
{
    public static async Task<IResult> Handle(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        ClaimsPrincipal user,
        IFeedService feedService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var (posts, meta) = await feedService.GetLikedPostsAsync(
            userId.Value,
            page ?? 1,
            pageSize ?? 20,
            cancellationToken
        );

        return Results.Ok(ApiResponse<IReadOnlyList<FeedPostResponse>>.Ok(posts, meta));
    }
}
