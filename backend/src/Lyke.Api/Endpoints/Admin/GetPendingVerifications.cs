using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Admin;

public static class GetPendingVerifications
{
    public static async Task<IResult> Handle(
        [FromQuery] VerificationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        ICreatorService creatorService = default!,
        CancellationToken cancellationToken = default
    )
    {
        var (verifications, meta) = await creatorService.GetPendingVerificationsAsync(
            status,
            page,
            pageSize,
            cancellationToken
        );
        return Results.Ok(
            ApiResponse<IReadOnlyList<PendingVerificationResponse>>.Ok(verifications, meta)
        );
    }
}
