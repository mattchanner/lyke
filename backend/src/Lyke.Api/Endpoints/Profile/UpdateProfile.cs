using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Profile;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Profile;

public static class UpdateProfile
{
    public static async Task<IResult> Handle(
        [FromBody] UpdateProfileRequest request,
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await profileService.UpdateProfileAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<UserProfileResponse>.Ok(result));
    }
}
