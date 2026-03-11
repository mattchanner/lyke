using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Media;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Media;

public static class UploadMediaBulk
{
    public static async Task<IResult> Handle(
        [FromForm] IFormFileCollection files,
        ClaimsPrincipal user,
        IMediaService mediaService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await mediaService.UploadMediaBulkAsync(
            userId.Value,
            files,
            cancellationToken
        );
        return Results.Ok(ApiResponse<BulkMediaUploadResponse>.Ok(result));
    }
}
