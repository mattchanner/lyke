using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Media;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Media;

public static class DeleteMedia
{
    public static async Task<IResult> Handle(
        [FromBody] MediaDeleteRequest request,
        ClaimsPrincipal user,
        IMediaService mediaService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        await mediaService.DeleteMediaAsync(userId.Value, request.MediaIds, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }
}
