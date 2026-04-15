using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Follow;

public static class GetUserFollowStatus
{
    public static async Task<IResult> Handle(
        Guid userId,
        ClaimsPrincipal user,
        IFollowService followService,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = UserIdExtractor.GetUserId(user);
        if (currentUserId == null)
            return Results.Unauthorized();

        var isFollowing = await followService.IsFollowingUserAsync(
            currentUserId.Value,
            userId,
            cancellationToken
        );
        return Results.Ok(ApiResponse<bool>.Ok(isFollowing));
    }
}
