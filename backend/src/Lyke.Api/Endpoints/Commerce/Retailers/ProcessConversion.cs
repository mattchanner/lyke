using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Commerce.Retailers;

public static class ProcessConversion
{
    public static async Task<IResult> Handle(
        Guid id,
        [FromBody] ConversionWebhookRequest request,
        ICommerceService commerceService,
        CancellationToken cancellationToken
    )
    {
        var success = await commerceService.ProcessConversionAsync(id, request, cancellationToken);
        if (!success)
        {
            return Results.BadRequest(
                ApiResponse.Fail("CONVERSION_FAILED", "Failed to process conversion")
            );
        }
        return Results.Ok(ApiResponse.Ok());
    }
}
