using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Posts;

public static class GetPost
{
    public static async Task<IResult> Handle(
        Guid id,
        ClaimsPrincipal user,
        IFeedService feedService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        var post = await feedService.GetPostAsync(id, userId, cancellationToken);
        return Results.Ok(ApiResponse<PostDetailResponse>.Ok(post));
    }
}
