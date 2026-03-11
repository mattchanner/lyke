using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Profile;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Profile;

public static class GetBodyProfile
{
    public static async Task<IResult> Handle(
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await profileService.GetBodyProfileAsync(userId.Value, cancellationToken);
        if (result == null)
        {
            return Results.NotFound(ApiResponse.Fail("NOT_FOUND", "Body profile not found"));
        }

        return Results.Ok(ApiResponse<BodyProfileResponse>.Ok(result));
    }
}
