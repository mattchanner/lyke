namespace Lyke.Application.DTOs.Retailer;

public record CreateCampaignRequest(
    Guid? ProductId,
    decimal BudgetAmount,
    List<string>? TargetBodyTypes,
    List<string>? TargetCategories,
    DateTime StartDate,
    DateTime EndDate
);
