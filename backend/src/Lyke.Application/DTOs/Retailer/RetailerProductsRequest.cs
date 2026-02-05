using Microsoft.AspNetCore.Mvc;

namespace Lyke.Application.DTOs.Retailer;

public class RetailerProductsRequest
{
    [FromQuery(Name = "category")]
    public string? Category { get; set; }

    [FromQuery(Name = "search")]
    public string? Search { get; set; }

    [FromQuery(Name = "isActive")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = 20;
}
