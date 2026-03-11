namespace Lyke.Application.DTOs.Retailer;

public record BodyProfileInsightsResponse(
    List<InsightBand> HeightBands,
    List<InsightBand> WeightBands,
    List<InsightBand> BodyTypes,
    List<InsightBand> FitPreferences,
    int MinimumGroupSize,
    int TotalEngagedUsers
);

public record InsightBand(string Label, int Count, decimal Percentage);
