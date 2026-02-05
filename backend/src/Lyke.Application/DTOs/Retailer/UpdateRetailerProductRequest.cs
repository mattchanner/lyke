namespace Lyke.Application.DTOs.Retailer;

public record UpdateRetailerProductRequest(
    string? Name,
    string? Description,
    string? Category,
    string? SubCategory,
    string? ProductUrl,
    decimal? Price,
    string? Currency,
    bool? IsActive);
