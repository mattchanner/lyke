namespace Lyke.Application.DTOs.Profile;

public record FrameSizeResponse(
    int Id,
    string Name,
    string? Description,
    int DisplayOrder
);
