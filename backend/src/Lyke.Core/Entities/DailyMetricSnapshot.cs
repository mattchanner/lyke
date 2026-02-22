namespace Lyke.Core.Entities;

public class DailyMetricSnapshot
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public required string Scope { get; set; }
    public Guid? ScopeEntityId { get; set; }
    public long Views { get; set; }
    public long Likes { get; set; }
    public long Saves { get; set; }
    public long Shares { get; set; }
    public long Clicks { get; set; }
    public long Conversions { get; set; }
    public decimal Revenue { get; set; }
    public decimal Earnings { get; set; }
    public int NewUsers { get; set; }
    public int PostsPublished { get; set; }
    public int SearchesExecuted { get; set; }
    public int FiltersApplied { get; set; }
    public DateTime ComputedAt { get; set; }
}
