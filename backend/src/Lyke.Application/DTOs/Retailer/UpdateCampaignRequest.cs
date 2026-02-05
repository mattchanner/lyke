namespace Lyke.Application.DTOs.Retailer;

public record UpdateCampaignRequest(
    decimal? BudgetAmount,
    List<string>? TargetBodyTypes,
    List<string>? TargetCategories,
    DateTime? StartDate,
    DateTime? EndDate,
    bool? IsActive);
