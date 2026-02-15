namespace Lyke.Application.DTOs.Retailer;

public record RetailerProfileResponse(
    Guid Id,
    string Name,
    string? LogoUrl,
    string? WebsiteUrl,
    string? ContactEmail,
    bool IsActive,
    int TotalProducts,
    int ActiveProducts,
    int TotalCampaigns,
    int ActiveCampaigns,
    DateTime CreatedAt,
    AffiliateConfigDto? AffiliateConfig);

public record AffiliateConfigDto(
    string? BaseUrl,
    string? AffiliateId,
    decimal? CommissionRate);
