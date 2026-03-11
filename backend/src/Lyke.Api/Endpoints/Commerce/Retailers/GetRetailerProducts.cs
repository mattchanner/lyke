using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Commerce.Retailers;

public static class GetRetailerProducts
{
    public static async Task<IResult> Handle(
        Guid id,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? category,
        ICommerceService commerceService,
        CancellationToken cancellationToken
    )
    {
        var (data, meta) = await commerceService.GetRetailerProductsAsync(
            id,
            page ?? 1,
            pageSize ?? 20,
            category,
            cancellationToken
        );
        return Results.Ok(ApiResponse<RetailerProductsResponse>.Ok(data, meta));
    }
}
