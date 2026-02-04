namespace Lyke.Application.DTOs.Media;

public record MediaDeleteRequest
{
    public required IReadOnlyList<string> MediaIds { get; init; }
}
