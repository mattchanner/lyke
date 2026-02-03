using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Auth;

public record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    UserType UserType = UserType.Shopper
);
