using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Profile;

public record UpdateBodyProfileRequest(
    int? HeightCm = null,
    decimal? WeightKg = null,
    int? BodyTypeId = null,
    FitPreference? FitPreference = null
);
