using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Auth;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Auth;

public static class ForgotPassword
{
    public static async Task<IResult> Handle(
        [FromBody] ForgotPasswordRequest request,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        await authService.ForgotPasswordAsync(request, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }
}
