namespace Lyke.Application.Interfaces;

public record SocialUserInfo(string ProviderKey, string Email, string? FirstName, string? LastName);

public interface ISocialTokenValidator
{
    Task<SocialUserInfo> ValidateAsync(
        string provider,
        string idToken,
        CancellationToken cancellationToken = default
    );
}
