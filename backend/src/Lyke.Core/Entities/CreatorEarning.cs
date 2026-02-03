using Lyke.Core.Enums;

namespace Lyke.Core.Entities;

public class CreatorEarning
{
    public Guid Id { get; set; }
    public Guid CreatorId { get; set; }
    public Guid ClickEventId { get; set; }
    public EarningType EarningType { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; set; }
    public EarningStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Creator Creator { get; set; } = null!;
    public ClickEvent ClickEvent { get; set; } = null!;
}
