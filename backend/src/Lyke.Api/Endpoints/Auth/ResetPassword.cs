using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Auth;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Auth;

public static class ResetPassword
{
    public static async Task<IResult> Handle(
        [FromBody] ResetPasswordRequest request,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        await authService.ResetPasswordAsync(request, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }
}
