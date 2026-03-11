using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;

namespace Lyke.Api.Endpoints.Auth;

public static class DeleteAccount
{
    public static async Task<IResult> Handle(
        ClaimsPrincipal user,
        IAuthService authService,
        IAuditService auditService,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        await authService.DeleteAccountAsync(userId.Value, cancellationToken);

        await auditService.LogAsync(
            userId.Value,
            AuditAction.AccountDeletion,
            targetUserId: userId.Value,
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString()
        );

        return Results.Ok(ApiResponse.Ok());
    }
}
