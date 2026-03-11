using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Creator;

public static class GetPublicCreatorProfile
{
    public static async Task<IResult> Handle(
        Guid creatorId,
        ICreatorService creatorService,
        CancellationToken cancellationToken
    )
    {
        var result = await creatorService.GetPublicCreatorProfileAsync(
            creatorId,
            cancellationToken
        );
        return Results.Ok(ApiResponse<PublicCreatorProfileResponse>.Ok(result));
    }
}
