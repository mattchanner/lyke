namespace Lyke.Core.Entities;

public class FitTag
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<PostFitTag> PostFitTags { get; set; } = new List<PostFitTag>();
}
