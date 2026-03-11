using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs.Retailer;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Retailer;

public static class ExportAnalyticsCsv
{
    public static async Task<IResult> Handle(
        [AsParameters] RetailerAnalyticsRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var csvBytes = await retailerService.ExportAnalyticsCsvAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.File(csvBytes, "text/csv", "analytics-export.csv");
    }
}
