namespace Lyke.Application.DTOs.Retailer;

public record RetailerProductResponse(
    Guid Id,
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
    int PostCount,
    int ClickCount,
    int ConversionCount,
    DateTime? LastSyncedAt,
    DateTime CreatedAt
);
