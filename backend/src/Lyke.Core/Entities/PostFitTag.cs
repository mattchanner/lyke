namespace Lyke.Core.Entities;

public class PostFitTag
{
    public Guid PostProductId { get; set; }
    public int FitTagId { get; set; }

    // Navigation properties
    public PostProduct PostProduct { get; set; } = null!;
    public FitTag FitTag { get; set; } = null!;
}
