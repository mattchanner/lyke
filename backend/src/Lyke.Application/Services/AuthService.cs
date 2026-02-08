using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs.Auth;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lyke.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly DbContext _dbContext;
    private readonly JwtSettings _jwtSettings;
    private readonly ISocialTokenValidator _socialTokenValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager,
        DbContext dbContext,
        IOptions<JwtSettings> jwtSettings,
        ISocialTokenValidator socialTokenValidator,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
        _socialTokenValidator = socialTokenValidator;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ValidationException("Email", "Email is already registered");
        }

        var user = new User
        {
            Email = request.Email,
            UserName = request.Email,
            UserType = request.UserType,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            throw new ValidationException(errors);
        }

        _logger.LogInformation("User {Email} registered successfully", request.Email);

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account is deactivated");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Failed login attempt for {Email}", request.Email);
            throw new UnauthorizedException("Invalid email or password");
        }

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var refreshTokens = _dbContext.Set<RefreshToken>();

        var storedToken = await refreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (storedToken == null)
        {
            throw new UnauthorizedException("Invalid refresh token");
        }

        if (!storedToken.IsActive)
        {
            // Token has been revoked or expired - revoke all tokens for this user (potential token theft)
            if (storedToken.IsRevoked)
            {
                _logger.LogWarning("Attempted reuse of revoked refresh token for user {UserId}", storedToken.UserId);
                await RevokeAllUserTokensAsync(storedToken.UserId, cancellationToken);
            }
            throw new UnauthorizedException("Invalid refresh token");
        }

        if (!storedToken.User.IsActive)
        {
            throw new UnauthorizedException("Account is deactivated");
        }

        // Revoke the old token
        storedToken.RevokedAt = DateTime.UtcNow;

        // Generate new tokens
        var response = await GenerateAuthResponseAsync(storedToken.User, cancellationToken);

        // Link old token to new token
        storedToken.ReplacedByToken = response.RefreshToken;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token rotated for user {UserId}", storedToken.UserId);

        return response;
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var refreshTokens = _dbContext.Set<RefreshToken>();

        var storedToken = await refreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (storedToken != null && storedToken.IsActive)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("User {UserId} logged out", storedToken.UserId);
        }
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Always return success to prevent email enumeration attacks
        if (user == null || !user.IsActive)
        {
            _logger.LogInformation("Password reset requested for non-existent or inactive email {Email}", request.Email);
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // TODO: Send email with reset token
        // For now, log the token (remove in production!)
        _logger.LogInformation("Password reset token generated for {Email}: {Token}", request.Email, token);

        // In production, this would send an email:
        // await _emailService.SendPasswordResetEmailAsync(user.Email, token);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            throw new ValidationException("Token", "Invalid or expired reset token");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            throw new ValidationException("Token", string.Join(", ", errors));
        }

        // Revoke all refresh tokens after password reset
        await RevokeAllUserTokensAsync(user.Id, cancellationToken);

        _logger.LogInformation("Password reset successful for {Email}", request.Email);
    }

    public async Task DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        // Revoke all refresh tokens
        await RevokeAllUserTokensAsync(userId, cancellationToken);

        // Soft delete - mark as inactive
        user.IsActive = false;
        user.Email = $"deleted_{userId}@deleted.local";
        user.NormalizedEmail = user.Email.ToUpperInvariant();
        user.UserName = user.Email;
        user.NormalizedUserName = user.Email.ToUpperInvariant();

        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Account {UserId} deleted (soft delete)", userId);
    }

    public async Task<AuthResponse> SocialLoginAsync(SocialLoginRequest request, CancellationToken cancellationToken = default)
    {
        var socialUser = await _socialTokenValidator.ValidateAsync(request.Provider, request.IdToken, cancellationToken);

        // 1. Check if this social login is already linked
        var existingUser = await _userManager.FindByLoginAsync(request.Provider, socialUser.ProviderKey);
        if (existingUser != null)
        {
            if (!existingUser.IsActive)
            {
                throw new UnauthorizedException("Account is deactivated");
            }

            _logger.LogInformation("Social login for existing linked user {Email} via {Provider}", existingUser.Email, request.Provider);
            return await GenerateAuthResponseAsync(existingUser, cancellationToken);
        }

        // 2. Check if email already exists (link the provider)
        var emailUser = await _userManager.FindByEmailAsync(socialUser.Email);
        if (emailUser != null)
        {
            if (!emailUser.IsActive)
            {
                throw new UnauthorizedException("Account is deactivated");
            }

            var loginInfo = new UserLoginInfo(request.Provider, socialUser.ProviderKey, request.Provider);
            var linkResult = await _userManager.AddLoginAsync(emailUser, loginInfo);
            if (!linkResult.Succeeded)
            {
                _logger.LogWarning("Failed to link {Provider} to existing user {Email}", request.Provider, socialUser.Email);
                throw new ValidationException("Provider", "Failed to link social account");
            }

            _logger.LogInformation("Linked {Provider} to existing user {Email}", request.Provider, emailUser.Email);
            return await GenerateAuthResponseAsync(emailUser, cancellationToken);
        }

        // 3. Create new user
        var newUser = new User
        {
            Email = socialUser.Email,
            UserName = socialUser.Email,
            EmailConfirmed = true,
            UserType = UserType.Shopper,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToArray();
            throw new ValidationException("User", string.Join(", ", errors));
        }

        var addLoginResult = await _userManager.AddLoginAsync(
            newUser, new UserLoginInfo(request.Provider, socialUser.ProviderKey, request.Provider));
        if (!addLoginResult.Succeeded)
        {
            _logger.LogWarning("Failed to add {Provider} login for new user {Email}", request.Provider, socialUser.Email);
        }

        _logger.LogInformation("Created new user {Email} via {Provider} social login", socialUser.Email, request.Provider);
        return await GenerateAuthResponseAsync(newUser, cancellationToken);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, cancellationToken);
        var expiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        return new AuthResponse(
            UserId: user.Id,
            Email: user.Email!,
            UserType: user.UserType,
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            AccessTokenExpiry: expiry
        );
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("user_type", user.UserType.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var refreshTokens = _dbContext.Set<RefreshToken>();

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        };

        await refreshTokens.AddAsync(refreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return token;
    }

    private async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var refreshTokens = _dbContext.Set<RefreshToken>();

        var activeTokens = await refreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
