namespace Lyke.Application.Configuration;

public class PrivacySettings
{
    public const string SectionName = "Privacy";

    public string CurrentPolicyVersion { get; set; } = "1.0";
    public DataRetentionSettings DataRetention { get; set; } = new();
    public AuditSettings Audit { get; set; } = new();
}

public class DataRetentionSettings
{
    public bool Enabled { get; set; } = true;
    public int RunIntervalHours { get; set; } = 24;
    public int PreferredRunHourUtc { get; set; } = 2;
    public int RefreshTokenGracePeriodDays { get; set; } = 7;
    public int AnonymizedClickEventRetentionDays { get; set; } = 730;
    public int ViewEngagementRetentionDays { get; set; } = 365;
    public int AuditLogRetentionDays { get; set; } = 1095;
    public int BatchSize { get; set; } = 1000;
}

public class AuditSettings
{
    public bool Enabled { get; set; } = true;
    public int RetentionDays { get; set; } = 1095;
}
