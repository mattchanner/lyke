using Lyke.Core.Enums;

namespace Lyke.Core.Entities;

public class PostProduct : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid ProductId { get; set; }
    public required string SizeWorn { get; set; }
    public string? FitNotes { get; set; }
    public FitRating? FitRating { get; set; }
    public string? StylingNotes { get; set; }

    // Navigation properties
    public Post Post { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ICollection<PostFitTag> FitTags { get; set; } = new List<PostFitTag>();
    public ICollection<ClickEvent> ClickEvents { get; set; } = new List<ClickEvent>();
}
