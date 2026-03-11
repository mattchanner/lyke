using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Profile;

public static class DeleteBodyProfile
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

        await profileService.DeleteBodyProfileAsync(userId.Value, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }
}
