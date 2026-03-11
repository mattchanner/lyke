using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Auth;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Auth;

public static class Logout
{
    public static async Task<IResult> Handle(
        [FromBody] RefreshTokenRequest request,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        await authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }
}
