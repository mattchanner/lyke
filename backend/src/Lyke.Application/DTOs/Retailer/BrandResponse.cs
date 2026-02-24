namespace Lyke.Application.DTOs.Retailer;

public record BrandResponse(
    Guid Id,
    string Name,
    string? LogoUrl,
    string? Description,
    int ProductCount
);
