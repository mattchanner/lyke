using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Media;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media")
            .WithTags("Media")
            .RequireAuthorization("CreatorOnly")
            .DisableAntiforgery();

        group.MapPost("/upload", UploadMediaAsync)
            .WithName("UploadMedia")
            .WithSummary("Upload a single media file (image or video)")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ApiResponse<MediaUploadResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group.MapPost("/upload/bulk", UploadMediaBulkAsync)
            .WithName("UploadMediaBulk")
            .WithSummary("Upload multiple media files (up to 10)")
            .Accepts<IFormFileCollection>("multipart/form-data")
            .Produces<ApiResponse<BulkMediaUploadResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group.MapDelete("/", DeleteMediaAsync)
            .WithName("DeleteMedia")
            .WithSummary("Delete media files by their IDs")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group.MapGet("/secure-url", GetSecureUrlAsync)
            .WithName("GetSecureMediaUrl")
            .WithSummary("Get a time-limited secure URL for media access")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> UploadMediaAsync(
        IFormFile file,
        ClaimsPrincipal user,
        IMediaService mediaService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var result = await mediaService.UploadMediaAsync(userId.Value, file, cancellationToken);
        return Results.Ok(ApiResponse<MediaUploadResponse>.Ok(result));
    }

    private static async Task<IResult> UploadMediaBulkAsync(
        IFormFileCollection files,
        ClaimsPrincipal user,
        IMediaService mediaService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var result = await mediaService.UploadMediaBulkAsync(userId.Value, files, cancellationToken);
        return Results.Ok(ApiResponse<BulkMediaUploadResponse>.Ok(result));
    }

    private static async Task<IResult> DeleteMediaAsync(
        [FromBody] MediaDeleteRequest request,
        ClaimsPrincipal user,
        IMediaService mediaService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        await mediaService.DeleteMediaAsync(userId.Value, request.MediaIds, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }

    private static async Task<IResult> GetSecureUrlAsync(
        [FromQuery] string url,
        ClaimsPrincipal user,
        IMediaService mediaService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var secureUrl = await mediaService.GetSecureUrlAsync(url, cancellationToken);
        return Results.Ok(ApiResponse<string>.Ok(secureUrl));
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
