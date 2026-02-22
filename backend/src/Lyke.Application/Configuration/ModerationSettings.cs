namespace Lyke.Application.Configuration;

public class ModerationSettings
{
    public const string SectionName = "Moderation";

    /// <summary>
    /// Number of reports before a post is automatically flagged. Default: 3
    /// </summary>
    public int AutoFlagThreshold { get; set; } = 3;
}
