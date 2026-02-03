namespace Lyke.Application.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public required string Secret { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int ExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 30;
}
