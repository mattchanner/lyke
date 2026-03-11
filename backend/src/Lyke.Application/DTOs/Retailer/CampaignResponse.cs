using Microsoft.AspNetCore.Mvc;

namespace Lyke.Application.DTOs.Retailer;

public record CampaignResponse(
    Guid Id,
    Guid? ProductId,
    string? ProductName,
    decimal BudgetAmount,
    decimal SpentAmount,
    string Status,
    List<string>? TargetBodyTypes,
    List<string>? TargetCategories,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    DateTime CreatedAt
);

public class CampaignListRequest
{
    [FromQuery(Name = "isActive")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = 20;
}
