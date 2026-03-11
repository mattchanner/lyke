using Lyke.Application.DTOs.Feed;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Feed;

public class FeedQueryParams
{
    [FromQuery(Name = "page")]
    public int? Page { get; set; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }

    [FromQuery(Name = "category")]
    public string? Category { get; set; }

    [FromQuery(Name = "retailerId")]
    public Guid? RetailerId { get; set; }

    [FromQuery(Name = "fitTagIds")]
    public string? FitTagIds { get; set; }

    [FromQuery(Name = "sortBy")]
    public FeedSortBy? SortBy { get; set; }

    [FromQuery(Name = "creatorId")]
    public Guid? CreatorId { get; set; }
}
