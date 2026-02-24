using Lyke.Core.Enums;

namespace Lyke.Core.Entities;

public class BodyProfileFitPreference
{
    public Guid BodyProfileId { get; set; }
    public FitPreference FitPreference { get; set; }

    // Navigation properties
    public BodyProfile BodyProfile { get; set; } = null!;
}
