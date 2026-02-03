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
    FitPreference? FitPreference,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record AnonymizedBodyProfileResponse(
    string HeightRange,
    string WeightRange,
    string BodyTypeName,
    FitPreference? FitPreference
);
