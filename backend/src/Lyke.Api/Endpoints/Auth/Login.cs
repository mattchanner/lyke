using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Auth;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Auth;

public static class Login
{
    public static async Task<IResult> Handle(
        [FromBody] LoginRequest request,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<AuthResponse>.Ok(result));
    }
}
