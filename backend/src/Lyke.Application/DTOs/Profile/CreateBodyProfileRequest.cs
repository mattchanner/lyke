using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Profile;

public record CreateBodyProfileRequest(
    int HeightCm,
    decimal WeightKg,
    int BodyTypeId,
    int? FrameSizeId = null,
    List<FitPreference>? FitPreferences = null
);
