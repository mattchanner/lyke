namespace Lyke.Application.DTOs.Creator;

public record UpdateCreatorProfileRequest(
    string? DisplayName,
    string? Bio,
    Dictionary<string, string>? SocialLinks
);
