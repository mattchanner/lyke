using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Auth;

namespace Lyke.Api.Endpoints.Auth;

public static class AuthEndpointRoutes
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/v1")
            .WithTags("Authentication")
            .RequireRateLimiting("auth");

        group
            .MapPost("/register", Register.Handle)
            .WithName("Register")
            .WithSummary("Register a new user account")
            .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group
            .MapPost("/login", Login.Handle)
            .WithName("Login")
            .WithSummary("Login with email and password")
            .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        group
            .MapPost("/refresh", RefreshToken.Handle)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token using refresh token")
            .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        group
            .MapPost("/logout", Logout.Handle)
            .WithName("Logout")
            .WithSummary("Logout and invalidate refresh token")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group
            .MapPost("/forgot-password", ForgotPassword.Handle)
            .WithName("ForgotPassword")
            .WithSummary("Request a password reset email")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group
            .MapPost("/reset-password", ResetPassword.Handle)
            .WithName("ResetPassword")
            .WithSummary("Reset password using reset token")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group
            .MapPost("/social-login", SocialLogin.Handle)
            .WithName("SocialLogin")
            .WithSummary("Login or register with a social provider (Google, Apple)")
            .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        group
            .MapDelete("/account", DeleteAccount.Handle)
            .WithName("DeleteAccount")
            .WithSummary("Delete user account (GDPR)")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group
            .MapPost("/resend-verification", ResendVerification.Handle)
            .WithName("ResendVerification")
            .WithSummary("Resend email verification link")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        return app;
    }
}
