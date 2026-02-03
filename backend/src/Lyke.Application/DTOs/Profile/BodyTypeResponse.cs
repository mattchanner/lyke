namespace Lyke.Application.DTOs.Profile;

public record BodyTypeResponse(
    int Id,
    string Name,
    string? Description,
    int DisplayOrder
);
