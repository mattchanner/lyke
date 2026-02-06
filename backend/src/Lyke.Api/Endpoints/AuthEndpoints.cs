using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Auth;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/v1").WithTags("Authentication");

        group
            .MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Register a new user account")
            .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group
            .MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Login with email and password")
            .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        group
            .MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token using refresh token")
            .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        group
            .MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("Logout and invalidate refresh token")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group
            .MapPost("/forgot-password", ForgotPasswordAsync)
            .WithName("ForgotPassword")
            .WithSummary("Request a password reset email")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group
            .MapPost("/reset-password", ResetPasswordAsync)
            .WithName("ResetPassword")
            .WithSummary("Reset password using reset token")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group
            .MapDelete("/account", DeleteAccountAsync)
            .WithName("DeleteAccount")
            .WithSummary("Delete user account (GDPR)")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.RefreshTokenAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    private static async Task<IResult> LogoutAsync(
        [FromBody] RefreshTokenRequest request,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        await authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }

    private static async Task<IResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        await authService.ForgotPasswordAsync(request, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }

    private static async Task<IResult> ResetPasswordAsync(
        [FromBody] ResetPasswordRequest request,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        await authService.ResetPasswordAsync(request, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }

    private static async Task<IResult> DeleteAccountAsync(
        ClaimsPrincipal user,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        var userIdClaim =
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        await authService.DeleteAccountAsync(userId, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }
}
