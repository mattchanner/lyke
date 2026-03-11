using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Follow;

public static class UnfollowCreator
{
    public static async Task<IResult> Handle(
        Guid creatorId,
        ClaimsPrincipal user,
        IFollowService followService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        await followService.UnfollowAsync(userId.Value, creatorId, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }
}
