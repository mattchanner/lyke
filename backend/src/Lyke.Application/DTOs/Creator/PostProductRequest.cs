using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Creator;

public record PostProductRequest(
    Guid ProductId,
    string SizeWorn,
    FitRating? FitRating,
    string? FitNotes,
    string? StylingNotes,
    List<int>? FitTagIds
);
