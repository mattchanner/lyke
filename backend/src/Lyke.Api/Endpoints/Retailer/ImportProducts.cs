using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Retailer;

public static class ImportProducts
{
    public static async Task<IResult> Handle(
        IFormFile file,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(ApiResponse.Fail("INVALID_FILE", "CSV file is required"));
        }

        using var stream = file.OpenReadStream();
        var result = await retailerService.ImportProductsAsync(
            userId.Value,
            stream,
            cancellationToken
        );
        return Results.Ok(ApiResponse<ImportProductsResponse>.Ok(result));
    }
}
