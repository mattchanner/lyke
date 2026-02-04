namespace Lyke.Application.DTOs.Creator;

public record RegisterCreatorRequest(
    string DisplayName,
    string? Bio,
    Dictionary<string, string>? SocialLinks
);
