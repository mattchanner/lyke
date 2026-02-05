namespace Lyke.Application.DTOs.Retailer;

public record FitInsightsResponse(
    List<ProductFitSummary> Products);

public record ProductFitSummary(
    Guid ProductId,
    string ProductName,
    string Category,
    int TotalReviews,
    string FitRecommendation,
    List<FitDistribution> FitDistribution,
    List<SizeFitBreakdown> SizeBreakdown);

public record FitDistribution(
    string Rating,
    int Count,
    decimal Percentage);

public record SizeFitBreakdown(
    string Size,
    int ReviewCount,
    string AverageFit);
