namespace Lyke.Application.Configuration;

public class AnalyticsSettings
{
    public const string SectionName = "Analytics";

    public bool AggregationEnabled { get; set; } = true;
    public int AggregationIntervalHours { get; set; } = 1;
    public int BackfillDays { get; set; } = 7;
    public int RetentionDays { get; set; } = 365;
}
