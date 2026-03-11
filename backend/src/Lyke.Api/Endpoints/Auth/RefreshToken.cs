using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Auth;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Auth;

public static class RefreshToken
{
    public static async Task<IResult> Handle(
        [FromBody] RefreshTokenRequest request,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.RefreshTokenAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<AuthResponse>.Ok(result));
    }
}
