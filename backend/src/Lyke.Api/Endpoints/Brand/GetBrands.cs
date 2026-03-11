using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Brand;

public static class GetBrands
{
    public static async Task<IResult> Handle(
        [AsParameters] BrandsRequest request,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var (brands, meta) = await retailerService.GetBrandsAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<BrandResponse>>.Ok(brands, meta));
    }
}
