using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Commerce.Products;

public static class GetProduct
{
    public static async Task<IResult> Handle(
        Guid id,
        ICommerceService commerceService,
        CancellationToken cancellationToken
    )
    {
        var product = await commerceService.GetProductAsync(id, cancellationToken);
        return Results.Ok(ApiResponse<ProductResponse>.Ok(product));
    }
}
