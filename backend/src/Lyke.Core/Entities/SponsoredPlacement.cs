namespace Lyke.Core.Entities;

public class SponsoredPlacement : BaseEntity
{
    public Guid RetailerId { get; set; }
    public Guid? ProductId { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public string? TargetBodyTypes { get; set; } // JSON array
    public string? TargetCategories { get; set; } // JSON array
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Retailer Retailer { get; set; } = null!;
    public Product? Product { get; set; }
}
