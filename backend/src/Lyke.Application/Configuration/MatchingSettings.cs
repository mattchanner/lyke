namespace Lyke.Application.Configuration;

public class MatchingSettings
{
    public const string SectionName = "Matching";

    /// <summary>
    /// Weight for height similarity (0-1). Default: 0.3
    /// </summary>
    public double HeightWeight { get; set; } = 0.3;

    /// <summary>
    /// Weight for weight similarity (0-1). Default: 0.3
    /// </summary>
    public double WeightWeight { get; set; } = 0.3;

    /// <summary>
    /// Weight for body type match (0-1). Default: 0.4
    /// </summary>
    public double BodyTypeWeight { get; set; } = 0.4;

    /// <summary>
    /// Maximum height difference in cm for 100% match. Default: 5
    /// </summary>
    public int HeightToleranceCm { get; set; } = 5;

    /// <summary>
    /// Maximum weight difference in kg for 100% match. Default: 5
    /// </summary>
    public decimal WeightToleranceKg { get; set; } = 5;

    /// <summary>
    /// Minimum similarity score to include in feed (0-1). Default: 0.3
    /// </summary>
    public double MinimumSimilarityScore { get; set; } = 0.3;

    /// <summary>
    /// Boost factor for sponsored content (1 = no boost). Default: 1.2
    /// </summary>
    public double SponsoredBoostFactor { get; set; } = 1.2;

    /// <summary>
    /// Decay factor for recency (posts older than this many days get reduced score). Default: 30
    /// </summary>
    public int RecencyDecayDays { get; set; } = 30;
}
