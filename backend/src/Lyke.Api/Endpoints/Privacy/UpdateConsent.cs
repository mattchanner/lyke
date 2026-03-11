using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Privacy;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;

namespace Lyke.Api.Endpoints.Privacy;

public static class UpdateConsent
{
    public static async Task<IResult> Handle(
        UpdateConsentRequest request,
        IPrivacyService privacyService,
        IAuditService auditService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await privacyService.UpdateConsentAsync(userId, request, cancellationToken);

        await auditService.LogAsync(
            userId,
            AuditAction.ConsentUpdate,
            targetUserId: userId,
            details: new { request.MarketingOptIn, request.AcceptPrivacyPolicy },
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString()
        );

        return Results.Ok(ApiResponse.Ok());
    }
}
