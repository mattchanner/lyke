namespace Lyke.Application.DTOs.Retailer;

public record RegisterRetailerRequest(
    string Name,
    string? LogoUrl,
    string? WebsiteUrl,
    string? ContactEmail);
