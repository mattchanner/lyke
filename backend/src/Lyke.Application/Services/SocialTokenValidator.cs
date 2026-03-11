using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Google.Apis.Auth;
using Lyke.Application.Configuration;
using Lyke.Application.Interfaces;
using Lyke.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lyke.Application.Services;

public class SocialTokenValidator : ISocialTokenValidator
{
    private readonly SocialAuthSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SocialTokenValidator> _logger;

    public SocialTokenValidator(
        IOptions<SocialAuthSettings> settings,
        HttpClient httpClient,
        ILogger<SocialTokenValidator> logger
    )
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SocialUserInfo> ValidateAsync(
        string provider,
        string idToken,
        CancellationToken cancellationToken = default
    )
    {
        return provider switch
        {
            "Google" => await ValidateGoogleTokenAsync(idToken, cancellationToken),
            "Apple" => await ValidateAppleTokenAsync(idToken, cancellationToken),
            _ => throw new ValidationException("Provider", $"Unsupported provider: {provider}"),
        };
    }

    private async Task<SocialUserInfo> ValidateGoogleTokenAsync(
        string idToken,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_settings.GoogleClientId],
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (string.IsNullOrEmpty(payload.Email))
            {
                throw new ValidationException(
                    "IdToken",
                    "Google token does not contain an email claim"
                );
            }

            return new SocialUserInfo(
                ProviderKey: payload.Subject,
                Email: payload.Email,
                FirstName: payload.GivenName,
                LastName: payload.FamilyName
            );
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google ID token");
            throw new UnauthorizedException("Invalid Google ID token");
        }
    }

    private async Task<SocialUserInfo> ValidateAppleTokenAsync(
        string idToken,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var jwksJson = await _httpClient.GetStringAsync(
                "https://appleid.apple.com/auth/keys",
                cancellationToken
            );

            var jwks = new JsonWebKeySet(jwksJson);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",
                ValidateAudience = true,
                ValidAudience = _settings.AppleAppId,
                ValidateLifetime = true,
                IssuerSigningKeys = jwks.GetSigningKeys(),
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(
                idToken,
                validationParameters,
                out var validatedToken
            );

            var subject = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(email))
            {
                throw new ValidationException(
                    "IdToken",
                    "Apple token does not contain required claims"
                );
            }

            // Apple doesn't include name in the ID token after first sign-in
            return new SocialUserInfo(
                ProviderKey: subject,
                Email: email,
                FirstName: null,
                LastName: null
            );
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Invalid Apple ID token");
            throw new UnauthorizedException("Invalid Apple ID token");
        }
    }
}
