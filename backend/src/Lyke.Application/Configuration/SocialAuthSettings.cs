namespace Lyke.Application.Configuration;

public class SocialAuthSettings
{
    public const string SectionName = "SocialAuth";

    public required string GoogleClientId { get; set; }
    public required string AppleAppId { get; set; }
}
