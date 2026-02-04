namespace Lyke.Application.DTOs.Commerce;

public record TrackClickResponse(
    Guid ClickId,
    string AffiliateUrl,
    string RetailerName,
    string ProductName
);
