namespace Lyke.Application.Configuration;

public class CommerceSettings
{
    public const string SectionName = "Commerce";

    /// <summary>
    /// Share of commission that goes to creator (0-1). Default: 0.7 (70%)
    /// </summary>
    public decimal CreatorCommissionShare { get; set; } = 0.7m;

    /// <summary>
    /// Default attribution window in days. Default: 30
    /// </summary>
    public int DefaultAttributionWindowDays { get; set; } = 30;

    /// <summary>
    /// Rate limit for click tracking per IP per minute. Default: 60
    /// </summary>
    public int ClickRateLimitPerMinute { get; set; } = 60;

    /// <summary>
    /// Enable click deduplication within session. Default: true
    /// </summary>
    public bool EnableClickDeduplication { get; set; } = true;

    /// <summary>
    /// Deduplication window in seconds. Default: 10
    /// </summary>
    public int DeduplicationWindowSeconds { get; set; } = 10;
}
