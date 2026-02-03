using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Auth;

public record AuthResponse(
    Guid UserId,
    string Email,
    UserType UserType,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry
);
