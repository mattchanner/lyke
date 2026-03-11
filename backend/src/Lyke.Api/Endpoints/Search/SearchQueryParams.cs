using Lyke.Application.DTOs.Feed;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Search;

public class SearchQueryParams
{
    [FromQuery(Name = "q")]
    public string? Q { get; set; }

    [FromQuery(Name = "page")]
    public int? Page { get; set; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }

    [FromQuery(Name = "type")]
    public SearchType? Type { get; set; }
}
