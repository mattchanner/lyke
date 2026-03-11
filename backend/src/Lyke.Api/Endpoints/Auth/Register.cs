using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Auth;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Auth;

public static class Register
{
    public static async Task<IResult> Handle(
        [FromBody] RegisterRequest request,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<AuthResponse>.Ok(result));
    }
}
