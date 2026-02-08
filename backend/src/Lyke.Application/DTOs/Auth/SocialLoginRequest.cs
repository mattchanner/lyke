namespace Lyke.Application.DTOs.Auth;

public record SocialLoginRequest(
    string Provider,
    string IdToken
);
