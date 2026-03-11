using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Creator;

public record CreatorPostsRequest(PostStatus? Status, int Page = 1, int PageSize = 20);
