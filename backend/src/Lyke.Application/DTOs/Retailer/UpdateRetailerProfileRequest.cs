namespace Lyke.Application.DTOs.Retailer;

public record UpdateRetailerProfileRequest(
    string? Name,
    string? LogoUrl,
    string? WebsiteUrl,
    string? ContactEmail,
    AffiliateConfigRequest? AffiliateConfig);

public record AffiliateConfigRequest(
    string? BaseUrl,
    string? AffiliateId,
    decimal? CommissionRate);
