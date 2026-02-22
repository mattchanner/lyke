using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Privacy;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;

namespace Lyke.Api.Endpoints;

public static class PrivacyEndpoints
{
    public static IEndpointRouteBuilder MapPrivacyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/privacy/v1")
            .WithTags("Privacy")
            .RequireAuthorization();

        group
            .MapGet("/export", ExportDataAsync)
            .WithName("ExportUserData")
            .WithSummary("Export all personal data (GDPR Art. 15/20)")
            .Produces<ApiResponse<DataExportResponse>>(StatusCodes.Status200OK);

        group
            .MapGet("/consent", GetConsentStatusAsync)
            .WithName("GetConsentStatus")
            .WithSummary("Get current consent preferences")
            .Produces<ApiResponse<ConsentStatusResponse>>(StatusCodes.Status200OK);

        group
            .MapPut("/consent", UpdateConsentAsync)
            .WithName("UpdateConsent")
            .WithSummary("Update consent preferences")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> ExportDataAsync(
        IPrivacyService privacyService,
        IAuditService auditService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var data = await privacyService.ExportUserDataAsync(userId, cancellationToken);

        await auditService.LogAsync(
            userId,
            AuditAction.DataExport,
            targetUserId: userId,
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

        return Results.Ok(ApiResponse<DataExportResponse>.Ok(data));
    }

    private static async Task<IResult> GetConsentStatusAsync(
        IPrivacyService privacyService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var status = await privacyService.GetConsentStatusAsync(userId, cancellationToken);
        return Results.Ok(ApiResponse<ConsentStatusResponse>.Ok(status));
    }

    private static async Task<IResult> UpdateConsentAsync(
        UpdateConsentRequest request,
        IPrivacyService privacyService,
        IAuditService auditService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await privacyService.UpdateConsentAsync(userId, request, cancellationToken);

        await auditService.LogAsync(
            userId,
            AuditAction.ConsentUpdate,
            targetUserId: userId,
            details: new
            {
                request.MarketingOptIn,
                request.AcceptPrivacyPolicy
            },
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

        return Results.Ok(ApiResponse.Ok());
    }
}
