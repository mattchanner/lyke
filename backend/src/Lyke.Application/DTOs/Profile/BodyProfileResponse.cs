using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Profile;

public record BodyProfileResponse(
    Guid Id,
    int HeightCm,
    string HeightDisplay,
    decimal WeightKg,
    string WeightDisplay,
    int BodyTypeId,
    string BodyTypeName,
    int? FrameSizeId,
    string? FrameSizeName,
    List<FitPreference> FitPreferences,
    bool NeedsProfileUpdate,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record AnonymizedBodyProfileResponse(
    string HeightRange,
    string WeightRange,
    string BodyTypeName,
    string? FrameSizeName,
    List<FitPreference> FitPreferences
);
