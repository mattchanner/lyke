using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Media;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Media;

public static class GetMediaStatus
{
    public static async Task<IResult> Handle(
        string mediaId,
        ClaimsPrincipal user,
        [FromServices] IMediaService mediaService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await mediaService.GetMediaStatusAsync(
            mediaId,
            userId.Value,
            cancellationToken
        );
        return result == null
            ? Results.NotFound(ApiResponse.Fail("MEDIA_NOT_FOUND", "Media not found"))
            : Results.Ok(ApiResponse<MediaUploadResponse>.Ok(result));
    }
}
