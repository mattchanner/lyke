using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Privacy;

namespace Lyke.Api.Endpoints.Privacy;

public static class PrivacyEndpointRoutes
{
    public static IEndpointRouteBuilder MapPrivacyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/privacy/v1").WithTags("Privacy").RequireAuthorization();

        group
            .MapGet("/export", ExportData.Handle)
            .WithName("ExportUserData")
            .WithSummary("Export all personal data (GDPR Art. 15/20)")
            .Produces<ApiResponse<DataExportResponse>>(StatusCodes.Status200OK);

        group
            .MapGet("/consent", GetConsentStatus.Handle)
            .WithName("GetConsentStatus")
            .WithSummary("Get current consent preferences")
            .Produces<ApiResponse<ConsentStatusResponse>>(StatusCodes.Status200OK);

        group
            .MapPut("/consent", UpdateConsent.Handle)
            .WithName("UpdateConsent")
            .WithSummary("Update consent preferences")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);

        return app;
    }
}
