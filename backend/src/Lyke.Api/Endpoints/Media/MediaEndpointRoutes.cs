using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Media;

namespace Lyke.Api.Endpoints.Media;

public static class MediaEndpointRoutes
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media/v1")
            .WithTags("Media")
            .RequireAuthorization("CreatorOnly")
            .DisableAntiforgery();

        group
            .MapPost("/upload", UploadMedia.Handle)
            .WithName("UploadMedia")
            .WithSummary("Upload a single media file (image or video)")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ApiResponse<MediaUploadResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapPost("/upload/bulk", UploadMediaBulk.Handle)
            .WithName("UploadMediaBulk")
            .WithSummary("Upload multiple media files (up to 10)")
            .Accepts<IFormFileCollection>("multipart/form-data")
            .Produces<ApiResponse<BulkMediaUploadResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapGet("/{mediaId}/status", GetMediaStatus.Handle)
            .WithName("GetMediaStatus")
            .WithSummary("Poll for media processing completion")
            .Produces<ApiResponse<MediaUploadResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapDelete("/", DeleteMedia.Handle)
            .WithName("DeleteMedia")
            .WithSummary("Delete media files by their IDs")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapGet("/secure-url", GetSecureUrl.Handle)
            .WithName("GetSecureMediaUrl")
            .WithSummary("Get a time-limited secure URL for media access")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        return app;
    }
}
