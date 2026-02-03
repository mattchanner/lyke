namespace Lyke.Core.Entities;

public class BodyType
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation properties
    public ICollection<BodyProfile> BodyProfiles { get; set; } = new List<BodyProfile>();
}
