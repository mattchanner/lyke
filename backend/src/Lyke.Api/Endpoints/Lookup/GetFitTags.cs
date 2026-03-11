using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Lookup;

public static class GetFitTags
{
    public static async Task<IResult> Handle(
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var result = await profileService.GetFitTagsAsync(cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<FitTagResponse>>.Ok(result));
    }
}
