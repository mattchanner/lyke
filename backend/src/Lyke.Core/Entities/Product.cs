namespace Lyke.Core.Entities;

public class Product : BaseEntity
{
    public Guid RetailerId { get; set; }
    public required string ExternalSku { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Category { get; set; }
    public string? SubCategory { get; set; }
    public string? ImageUrls { get; set; } // JSON array
    public required string ProductUrl { get; set; }
    public decimal Price { get; set; }
    public required string Currency { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastSyncedAt { get; set; }

    // Navigation properties
    public Retailer Retailer { get; set; } = null!;
    public ICollection<PostProduct> PostProducts { get; set; } = new List<PostProduct>();
}
