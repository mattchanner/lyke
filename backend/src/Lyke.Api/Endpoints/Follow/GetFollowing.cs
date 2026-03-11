using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Follow;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Follow;

public static class GetFollowing
{
    public static async Task<IResult> Handle(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        ClaimsPrincipal user,
        IFollowService followService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var (creators, meta) = await followService.GetFollowingAsync(
            userId.Value,
            page ?? 1,
            pageSize ?? 20,
            cancellationToken
        );

        return Results.Ok(ApiResponse<IReadOnlyList<FollowedCreatorResponse>>.Ok(creators, meta));
    }
}
