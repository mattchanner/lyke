using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;
using Lyke.Application.DTOs.Feed;

namespace Lyke.Api.Endpoints.Commerce.Products;

public static class ProductEndpointRoutes
{
    public static void MapProductEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/commerce/v1/products").WithTags("Products");

        group
            .MapGet("/{id:guid}", GetProduct.Handle)
            .WithName("GetProduct")
            .WithSummary("Get product details by ID")
            .Produces<ApiResponse<ProductResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group
            .MapGet("/{id:guid}/posts", GetProductPosts.Handle)
            .WithName("GetProductPosts")
            .WithSummary("Get published posts linked to a product")
            .Produces<ApiResponse<IReadOnlyList<FeedPostResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group
            .MapGet("/search", SearchProducts.Handle)
            .WithName("SearchProducts")
            .WithSummary("Search products")
            .Produces<ApiResponse<IReadOnlyList<ProductResponse>>>(StatusCodes.Status200OK)
            .RequireAuthorization();
    }
}
