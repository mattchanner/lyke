using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Creator;

public static class GetEarningsHistory
{
    public static async Task<IResult> Handle(
        [AsParameters] EarningsHistoryRequest request,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var (earnings, meta) = await creatorService.GetEarningsHistoryAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<IReadOnlyList<EarningDetailResponse>>.Ok(earnings, meta));
    }
}
