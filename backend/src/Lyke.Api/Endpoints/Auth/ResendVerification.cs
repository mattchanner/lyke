using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Auth;

public static class ResendVerification
{
    public static async Task<IResult> Handle(
        ClaimsPrincipal user,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        await authService.ResendVerificationEmailAsync(userId.Value, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }
}
