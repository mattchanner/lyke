using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Profile;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Profile;

public static class UploadProfileImage
{
    private static readonly HashSet<string> AllowedImageTypes = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "image/jpeg",
        "image/png",
        "image/webp",
    };

    private const long MaxImageSize = 5 * 1024 * 1024; // 5MB

    public static async Task<IResult> Handle(
        [FromForm] IFormFile file,
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        if (file == null || file.Length == 0)
            return Results.BadRequest(ApiResponse.Fail("VALIDATION_ERROR", "No file provided"));

        if (!AllowedImageTypes.Contains(file.ContentType))
            return Results.BadRequest(
                ApiResponse.Fail("VALIDATION_ERROR", "Only JPEG, PNG, and WebP images are allowed")
            );

        if (file.Length > MaxImageSize)
            return Results.BadRequest(
                ApiResponse.Fail("VALIDATION_ERROR", "Image must be 5MB or smaller")
            );

        using var stream = file.OpenReadStream();
        var result = await profileService.UploadProfileImageAsync(
            userId.Value,
            stream,
            file.ContentType,
            cancellationToken
        );
        return Results.Ok(ApiResponse<UserProfileResponse>.Ok(result));
    }
}
