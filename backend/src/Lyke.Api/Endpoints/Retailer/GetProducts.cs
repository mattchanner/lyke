using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Retailer;

public static class GetProducts
{
    public static async Task<IResult> Handle(
        [AsParameters] RetailerProductsRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var (products, meta) = await retailerService.GetProductsAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<IReadOnlyList<RetailerProductResponse>>.Ok(products, meta));
    }
}
