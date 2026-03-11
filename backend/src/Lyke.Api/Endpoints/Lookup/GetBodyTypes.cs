using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Profile;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Lookup;

public static class GetBodyTypes
{
    public static async Task<IResult> Handle(
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var result = await profileService.GetBodyTypesAsync(cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<BodyTypeResponse>>.Ok(result));
    }
}
