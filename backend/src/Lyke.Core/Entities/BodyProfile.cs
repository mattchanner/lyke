using Lyke.Core.Enums;

namespace Lyke.Core.Entities;

public class BodyProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public int HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public int BodyTypeId { get; set; }
    public FitPreference? FitPreference { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public BodyType BodyType { get; set; } = null!;
}
