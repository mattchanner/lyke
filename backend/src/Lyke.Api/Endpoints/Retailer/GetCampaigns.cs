using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Retailer;

public static class GetCampaigns
{
    public static async Task<IResult> Handle(
        [AsParameters] CampaignListRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var (campaigns, meta) = await retailerService.GetCampaignsAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<IReadOnlyList<CampaignResponse>>.Ok(campaigns, meta));
    }
}
