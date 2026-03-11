using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Commerce.Products;

public static class SearchProducts
{
    public static async Task<IResult> Handle(
        [AsParameters] ProductSearchQueryParams queryParams,
        ICommerceService commerceService,
        CancellationToken cancellationToken
    )
    {
        var request = new ProductSearchRequest(
            queryParams.Q ?? "",
            queryParams.RetailerId,
            queryParams.Category,
            queryParams.Page ?? 1,
            queryParams.PageSize ?? 20
        );

        var (products, meta) = await commerceService.SearchProductsAsync(
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<IReadOnlyList<ProductResponse>>.Ok(products, meta));
    }
}
