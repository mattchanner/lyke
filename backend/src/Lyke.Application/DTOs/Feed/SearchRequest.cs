namespace Lyke.Application.DTOs.Feed;

public record SearchRequest(string Query, int Page = 1, int PageSize = 20, SearchType? Type = null);

public enum SearchType
{
    All = 0,
    Posts = 1,
    Products = 2,
    Creators = 3,
}
