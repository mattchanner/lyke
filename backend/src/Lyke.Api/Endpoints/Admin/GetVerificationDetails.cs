using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Admin;

public static class GetVerificationDetails
{
    public static async Task<IResult> Handle(
        Guid creatorId,
        ICreatorService creatorService,
        CancellationToken cancellationToken
    )
    {
        var result = await creatorService.GetVerificationDetailsAsync(creatorId, cancellationToken);
        return Results.Ok(ApiResponse<PendingVerificationResponse>.Ok(result));
    }
}
