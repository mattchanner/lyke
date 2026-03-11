using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Privacy;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Privacy;

public static class GetConsentStatus
{
    public static async Task<IResult> Handle(
        IPrivacyService privacyService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var status = await privacyService.GetConsentStatusAsync(userId, cancellationToken);
        return Results.Ok(ApiResponse<ConsentStatusResponse>.Ok(status));
    }
}
