using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Posts;

public static class Engage
{
    public static async Task<IResult> Handle(
        Guid id,
        [FromBody] EngageRequest request,
        ClaimsPrincipal user,
        IFeedService feedService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        await feedService.EngageAsync(id, userId.Value, request, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }
}
