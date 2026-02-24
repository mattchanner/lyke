using Microsoft.AspNetCore.Mvc;

namespace Lyke.Application.DTOs.Retailer;

public class BrandsRequest
{
    [FromQuery(Name = "search")]
    public string? Search { get; set; }

    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = 20;
}
