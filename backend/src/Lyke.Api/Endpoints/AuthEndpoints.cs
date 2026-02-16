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
            .MapPost("/social-login", SocialLoginAsync)
            .WithName("SocialLogin")
            .WithSummary("Login or register with a social provider (Google, Apple)")
            .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        group
            .MapGet("/verify-email", VerifyEmailAsync)
            .WithName("VerifyEmail")
            .WithSummary("Verify email address from email link")
            .Produces<string>(StatusCodes.Status200OK, "text/html")
            .AllowAnonymous()
            .ExcludeFromDescription();

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

    private static async Task<IResult> SocialLoginAsync(
        [FromBody] SocialLoginRequest request,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.SocialLoginAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    private static async Task<IResult> VerifyEmailAsync(
        [FromQuery] string email,
        [FromQuery] string token,
        IAuthService authService,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await authService.VerifyEmailAsync(email, token, cancellationToken);
            return Results.Content(VerifyEmailSuccessHtml, "text/html");
        }
        catch
        {
            return Results.Content(VerifyEmailErrorHtml, "text/html");
        }
    }

    private const string VerifyEmailSuccessHtml = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Email Verified - LYKE</title>
            <link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;600;700&display=swap" rel="stylesheet">
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body { font-family: 'Plus Jakarta Sans', sans-serif; background: #FFF8F6; min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 24px; color: #1A1A1A; }
                .card { background: #fff; border-radius: 16px; padding: 48px 32px; max-width: 420px; width: 100%; text-align: center; box-shadow: 0 4px 24px rgba(0,0,0,0.06); }
                .icon { width: 64px; height: 64px; background: #E8F5E9; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto 24px; }
                .icon svg { width: 32px; height: 32px; color: #2E7D32; }
                h1 { font-size: 24px; font-weight: 700; margin-bottom: 12px; }
                p { font-size: 16px; color: #666; line-height: 1.5; margin-bottom: 32px; }
                .brand { font-size: 14px; font-weight: 600; color: #C45D3E; }
            </style>
        </head>
        <body>
            <div class="card">
                <div class="icon">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
                    </svg>
                </div>
                <h1>Email Verified!</h1>
                <p>Your email has been confirmed. You can close this page and return to the app.</p>
                <span class="brand">LYKE</span>
            </div>
        </body>
        </html>
        """;

    private const string VerifyEmailErrorHtml = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Verification Failed - LYKE</title>
            <link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;600;700&display=swap" rel="stylesheet">
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body { font-family: 'Plus Jakarta Sans', sans-serif; background: #FFF8F6; min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 24px; color: #1A1A1A; }
                .card { background: #fff; border-radius: 16px; padding: 48px 32px; max-width: 420px; width: 100%; text-align: center; box-shadow: 0 4px 24px rgba(0,0,0,0.06); }
                .icon { width: 64px; height: 64px; background: #FBE9E7; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto 24px; }
                .icon svg { width: 32px; height: 32px; color: #C45D3E; }
                h1 { font-size: 24px; font-weight: 700; margin-bottom: 12px; }
                p { font-size: 16px; color: #666; line-height: 1.5; margin-bottom: 32px; }
                .brand { font-size: 14px; font-weight: 600; color: #C45D3E; }
            </style>
        </head>
        <body>
            <div class="card">
                <div class="icon">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                </div>
                <h1>Verification Failed</h1>
                <p>This link is invalid or has expired. Please request a new verification email from the app.</p>
                <span class="brand">LYKE</span>
            </div>
        </body>
        </html>
        """;

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
