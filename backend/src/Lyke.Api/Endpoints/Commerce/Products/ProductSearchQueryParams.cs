using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Commerce.Products;

public class ProductSearchQueryParams
{
    [FromQuery(Name = "query")]
    public string? Q { get; set; }

    [FromQuery(Name = "retailerId")]
    public Guid? RetailerId { get; set; }

    [FromQuery(Name = "category")]
    public string? Category { get; set; }

    [FromQuery(Name = "page")]
    public int? Page { get; set; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }
}
