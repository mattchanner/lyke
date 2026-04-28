using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Profile;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Profile;

public static class GetPublicUserProfile
{
    public static async Task<IResult> Handle(
        Guid userId,
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var result = await profileService.GetPublicUserProfileAsync(userId, cancellationToken);
        return Results.Ok(ApiResponse<PublicUserProfileResponse>.Ok(result));
    }
}
