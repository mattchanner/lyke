using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;

namespace Lyke.Api.Endpoints.Brand;

public static class BrandEndpointRoutes
{
    public static IEndpointRouteBuilder MapBrandEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/brands/v1").WithTags("Brands").RequireAuthorization();

        group
            .MapGet("/", GetBrands.Handle)
            .WithName("GetBrands")
            .WithSummary("List active brands with product counts")
            .Produces<ApiResponse<IReadOnlyList<BrandResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        return app;
    }
}
