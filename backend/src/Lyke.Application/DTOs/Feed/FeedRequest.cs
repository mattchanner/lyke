namespace Lyke.Application.DTOs.Feed;

public record FeedRequest(
    int Page = 1,
    int PageSize = 20,
    string? Category = null,
    Guid? RetailerId = null,
    List<int>? FitTagIds = null,
    FeedSortBy SortBy = FeedSortBy.Relevance,
    Guid? CreatorId = null
);

public enum FeedSortBy
{
    Relevance = 0,
    Recent = 1,
    MostLiked = 2,
    Following = 3,
}
