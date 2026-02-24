using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints;

public static class BrandEndpoints
{
    public static IEndpointRouteBuilder MapBrandEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/brands/v1")
            .WithTags("Brands")
            .RequireAuthorization();

        group
            .MapGet("/", GetBrandsAsync)
            .WithName("GetBrands")
            .WithSummary("List active brands with product counts")
            .Produces<ApiResponse<IReadOnlyList<BrandResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> GetBrandsAsync(
        [AsParameters] BrandsRequest request,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var (brands, meta) = await retailerService.GetBrandsAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<BrandResponse>>.Ok(brands, meta));
    }
}
