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
    private readonly PrivacySettings _privacySettings;
    private readonly ISocialTokenValidator _socialTokenValidator;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager,
        DbContext dbContext,
        IOptions<JwtSettings> jwtSettings,
        IOptions<PrivacySettings> privacySettings,
        ISocialTokenValidator socialTokenValidator,
        IEmailService emailService,
        ILogger<AuthService> logger
    )
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
        _privacySettings = privacySettings.Value;
        _socialTokenValidator = socialTokenValidator;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default
    )
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
            IsActive = true,
            PrivacyPolicyAcceptedAt = DateTime.UtcNow,
            PrivacyPolicyVersion = _privacySettings.CurrentPolicyVersion,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result
                .Errors.GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            throw new ValidationException(errors);
        }

        _logger.LogInformation("User {Email} registered successfully", request.Email);

        _ = _emailService.SendWelcomeEmailAsync(
            user.Email!,
            user.UserType.ToString(),
            cancellationToken
        );

        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        _ = _emailService.SendEmailVerificationAsync(user.Email!, emailToken, cancellationToken);

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default
    )
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

        // Check if account is currently locked out
        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            _logger.LogWarning("Locked-out login attempt for {Email}", request.Email);
            throw new AccountLockedException(lockoutEnd);
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            await _userManager.AccessFailedAsync(user);

            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                _logger.LogWarning("Account locked after failed login for {Email}", request.Email);
                throw new AccountLockedException(lockoutEnd);
            }

            _logger.LogWarning("Failed login attempt for {Email}", request.Email);
            throw new UnauthorizedException("Invalid email or password");
        }

        // Successful login — reset failed access count
        await _userManager.ResetAccessFailedCountAsync(user);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default
    )
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
                _logger.LogWarning(
                    "Attempted reuse of revoked refresh token for user {UserId}",
                    storedToken.UserId
                );
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

    public async Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        var refreshTokens = _dbContext.Set<RefreshToken>();

        var storedToken = await refreshTokens.FirstOrDefaultAsync(
            rt => rt.Token == refreshToken,
            cancellationToken
        );

        if (storedToken != null && storedToken.IsActive)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("User {UserId} logged out", storedToken.UserId);
        }
    }

    public async Task ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Always return success to prevent email enumeration attacks
        if (user == null || !user.IsActive)
        {
            _logger.LogInformation(
                "Password reset requested for non-existent or inactive email {Email}",
                request.Email
            );
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        _ = _emailService.SendPasswordResetEmailAsync(user.Email!, token, cancellationToken);

        _logger.LogInformation("Password reset email sent to {Email}", request.Email);
    }

    public async Task ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            throw new ValidationException("Token", "Invalid or expired reset token");
        }

        var result = await _userManager.ResetPasswordAsync(
            user,
            request.Token,
            request.NewPassword
        );
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

        // 1. Hard-delete refresh tokens
        var refreshTokens = await _dbContext
            .Set<RefreshToken>()
            .Where(rt => rt.UserId == userId)
            .ToListAsync(cancellationToken);
        _dbContext.Set<RefreshToken>().RemoveRange(refreshTokens);

        // 2. Hard-delete body profile
        var bodyProfile = await _dbContext
            .Set<BodyProfile>()
            .FirstOrDefaultAsync(bp => bp.UserId == userId, cancellationToken);
        if (bodyProfile != null)
        {
            _dbContext.Set<BodyProfile>().Remove(bodyProfile);
        }

        // 3. Hard-delete engagements (likes/saves)
        var engagements = await _dbContext
            .Set<Engagement>()
            .Where(e => e.UserId == userId)
            .ToListAsync(cancellationToken);
        _dbContext.Set<Engagement>().RemoveRange(engagements);

        // 4. Anonymize click events (keep for aggregate analytics)
        var clickEvents = await _dbContext
            .Set<ClickEvent>()
            .Where(ce => ce.UserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var ce in clickEvents)
        {
            ce.UserId = null;
            ce.SessionId = null;
        }

        // 5. If creator: cascade delete all creator data
        var creator = await _dbContext
            .Set<Creator>()
            .Include(c => c.Posts)
                .ThenInclude(p => p.PostProducts)
                    .ThenInclude(pp => pp.FitTags)
            .Include(c => c.Posts)
                .ThenInclude(p => p.PostProducts)
                    .ThenInclude(pp => pp.ClickEvents)
            .Include(c => c.Posts)
                .ThenInclude(p => p.Engagements)
            .Include(c => c.Posts)
                .ThenInclude(p => p.ClickEvents)
            .Include(c => c.Earnings)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (creator != null)
        {
            foreach (var post in creator.Posts)
            {
                // Anonymize click events on creator's posts (keep for analytics)
                foreach (var postClick in post.ClickEvents)
                {
                    postClick.UserId = null;
                    postClick.SessionId = null;
                }

                // Hard-delete engagements on creator's posts
                _dbContext.Set<Engagement>().RemoveRange(post.Engagements);

                foreach (var pp in post.PostProducts)
                {
                    // Anonymize click events on post products
                    foreach (var ppClick in pp.ClickEvents)
                    {
                        ppClick.UserId = null;
                        ppClick.SessionId = null;
                    }

                    // Hard-delete fit tags
                    _dbContext.Set<PostFitTag>().RemoveRange(pp.FitTags);
                }

                // Hard-delete post products
                _dbContext.Set<PostProduct>().RemoveRange(post.PostProducts);
            }

            // Hard-delete earnings
            _dbContext.Set<CreatorEarning>().RemoveRange(creator.Earnings);

            // Hard-delete posts
            _dbContext.Set<Post>().RemoveRange(creator.Posts);

            // Hard-delete creator
            _dbContext.Set<Creator>().Remove(creator);
        }

        // 6. If retailer: deactivate and clear personal fields
        var retailer = await _dbContext
            .Set<Retailer>()
            .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);
        if (retailer != null)
        {
            retailer.IsActive = false;
            retailer.ContactEmail = null;
        }

        // 7. Anonymize user record
        user.IsActive = false;
        user.Email = $"deleted_{userId}@deleted.local";
        user.NormalizedEmail = user.Email.ToUpperInvariant();
        user.UserName = user.Email;
        user.NormalizedUserName = user.Email.ToUpperInvariant();
        user.PhoneNumber = null;
        user.PrivacyPolicyAcceptedAt = null;
        user.PrivacyPolicyVersion = null;
        user.MarketingOptIn = false;

        await _userManager.UpdateAsync(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account {UserId} deleted with GDPR cascade", userId);
    }

    public async Task<AuthResponse> SocialLoginAsync(
        SocialLoginRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var socialUser = await _socialTokenValidator.ValidateAsync(
            request.Provider,
            request.IdToken,
            cancellationToken
        );

        // 1. Check if this social login is already linked
        var existingUser = await _userManager.FindByLoginAsync(
            request.Provider,
            socialUser.ProviderKey
        );
        if (existingUser != null)
        {
            if (!existingUser.IsActive)
            {
                throw new UnauthorizedException("Account is deactivated");
            }

            _logger.LogInformation(
                "Social login for existing linked user {Email} via {Provider}",
                existingUser.Email,
                request.Provider
            );
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

            var loginInfo = new UserLoginInfo(
                request.Provider,
                socialUser.ProviderKey,
                request.Provider
            );
            var linkResult = await _userManager.AddLoginAsync(emailUser, loginInfo);
            if (!linkResult.Succeeded)
            {
                _logger.LogWarning(
                    "Failed to link {Provider} to existing user {Email}",
                    request.Provider,
                    socialUser.Email
                );
                throw new ValidationException("Provider", "Failed to link social account");
            }

            _logger.LogInformation(
                "Linked {Provider} to existing user {Email}",
                request.Provider,
                emailUser.Email
            );
            return await GenerateAuthResponseAsync(emailUser, cancellationToken);
        }

        // 3. Create new user
        var newUser = new User
        {
            Email = socialUser.Email,
            UserName = socialUser.Email,
            EmailConfirmed = true,
            UserType = UserType.Shopper,
            IsActive = true,
        };

        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToArray();
            throw new ValidationException("User", string.Join(", ", errors));
        }

        var addLoginResult = await _userManager.AddLoginAsync(
            newUser,
            new UserLoginInfo(request.Provider, socialUser.ProviderKey, request.Provider)
        );
        if (!addLoginResult.Succeeded)
        {
            _logger.LogWarning(
                "Failed to add {Provider} login for new user {Email}",
                request.Provider,
                socialUser.Email
            );
        }

        _logger.LogInformation(
            "Created new user {Email} via {Provider} social login",
            socialUser.Email,
            request.Provider
        );
        return await GenerateAuthResponseAsync(newUser, cancellationToken);
    }

    public async Task VerifyEmailAsync(
        string email,
        string token,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            throw new ValidationException("Token", "Invalid or expired verification token");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            throw new ValidationException("Token", string.Join(", ", errors));
        }

        _logger.LogInformation("Email verified for {Email}", email);
    }

    public async Task ResendVerificationEmailAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        if (user.EmailConfirmed)
        {
            throw new ValidationException("Email", "Email is already verified");
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendEmailVerificationAsync(user.Email!, token, cancellationToken);

        _logger.LogInformation("Verification email resent for {Email}", user.Email);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(
        User user,
        CancellationToken cancellationToken
    )
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
            new Claim("user_type", user.UserType.ToString()),
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

    private async Task<string> GenerateRefreshTokenAsync(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var refreshTokens = _dbContext.Set<RefreshToken>();

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
        };

        await refreshTokens.AddAsync(refreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return token;
    }

    private async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var refreshTokens = _dbContext.Set<RefreshToken>();

        var activeTokens = await refreshTokens
            .Where(rt =>
                rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow
            )
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
