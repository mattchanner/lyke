namespace Lyke.Application.Configuration;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string ConnectionString { get; set; } = string.Empty;

    public required string SenderAddress { get; set; }

    public string SenderDisplayName { get; set; } = "LYKE";

    public required string AppBaseUrl { get; set; }

    public bool DryRun { get; set; } = false;

    public int RateLimitMaxPerWindow { get; set; } = 5;

    public int RateLimitWindowMinutes { get; set; } = 15;
}
