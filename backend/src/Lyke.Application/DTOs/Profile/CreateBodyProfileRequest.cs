using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Profile;

public record CreateBodyProfileRequest(
    int HeightCm,
    decimal WeightKg,
    int BodyTypeId,
    FitPreference? FitPreference = null
);
