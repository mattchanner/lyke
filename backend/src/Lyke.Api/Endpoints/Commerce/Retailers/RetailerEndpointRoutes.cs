using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;

namespace Lyke.Api.Endpoints.Commerce.Retailers;

public static class RetailerEndpointRoutes
{
    public static void MapRetailerEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/commerce/v1/retailers").WithTags("Retailers");

        group
            .MapGet("/", GetRetailers.Handle)
            .WithName("GetRetailers")
            .WithSummary("Get all active retailers")
            .Produces<ApiResponse<IReadOnlyList<RetailerResponse>>>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group
            .MapGet("/{id:guid}/products", GetRetailerProducts.Handle)
            .WithName("GetRetailerProducts")
            .WithSummary("Get retailer products (paginated)")
            .Produces<ApiResponse<RetailerProductsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group
            .MapPost("/{id:guid}/conversions", ProcessConversion.Handle)
            .WithName("ProcessConversion")
            .WithSummary("Webhook for retailer to report conversions")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();
    }
}
