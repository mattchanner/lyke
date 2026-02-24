using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Profile;

public record UpdateBodyProfileRequest(
    int? HeightCm = null,
    decimal? WeightKg = null,
    int? BodyTypeId = null,
    int? FrameSizeId = null,
    List<FitPreference>? FitPreferences = null
);
