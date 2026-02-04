namespace Lyke.Application.DTOs.Commerce;

public record RetailerResponse(
    Guid Id,
    string Name,
    string? LogoUrl,
    string? WebsiteUrl,
    bool IsActive,
    int ProductCount,
    int PostCount
);

public record RetailerProductsResponse(
    Guid RetailerId,
    string RetailerName,
    List<ProductResponse> Products
);
