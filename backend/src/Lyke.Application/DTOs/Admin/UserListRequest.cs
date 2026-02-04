using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Admin;

public record UserListRequest(
    UserType? UserType = null,
    bool? IsActive = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20
);
