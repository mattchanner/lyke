using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Commerce.Retailers;

public static class GetRetailers
{
    public static async Task<IResult> Handle(
        ICommerceService commerceService,
        CancellationToken cancellationToken
    )
    {
        var retailers = await commerceService.GetRetailersAsync(cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<RetailerResponse>>.Ok(retailers));
    }
}
