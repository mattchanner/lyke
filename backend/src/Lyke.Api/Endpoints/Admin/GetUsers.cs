using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Admin;

public static class GetUsers
{
    public static async Task<IResult> Handle(
        [FromQuery] UserType? userType,
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        IAdminService adminService = default!,
        CancellationToken cancellationToken = default
    )
    {
        var request = new UserListRequest(userType, isActive, search, page, pageSize);
        var (users, meta) = await adminService.GetUsersAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<UserListResponse>>.Ok(users, meta));
    }
}
