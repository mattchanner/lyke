using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Media;

public static class GetSecureUrl
{
    public static async Task<IResult> Handle(
        [FromQuery] string url,
        ClaimsPrincipal user,
        IMediaService mediaService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var secureUrl = await mediaService.GetSecureUrlAsync(url, cancellationToken);
        return Results.Ok(ApiResponse<string>.Ok(secureUrl));
    }
}
