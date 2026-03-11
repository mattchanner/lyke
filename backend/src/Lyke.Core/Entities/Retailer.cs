namespace Lyke.Core.Entities;

public class Retailer : BaseEntity
{
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? ContactEmail { get; set; }
    public string? AffiliateConfig { get; set; } // JSON
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<SponsoredPlacement> SponsoredPlacements { get; set; } =
        new List<SponsoredPlacement>();
}
