namespace Lyke.Application.DTOs.Commerce;

public record ProductResponse(
    Guid Id,
    Guid RetailerId,
    string RetailerName,
    string? RetailerLogoUrl,
    string ExternalSku,
    string Name,
    string? Description,
    string Category,
    string? SubCategory,
    List<string> ImageUrls,
    string ProductUrl,
    decimal Price,
    string Currency,
    bool IsActive,
    int PostCount
);

public record ProductSearchRequest(
    string Query,
    Guid? RetailerId = null,
    string? Category = null,
    int Page = 1,
    int PageSize = 20
);
