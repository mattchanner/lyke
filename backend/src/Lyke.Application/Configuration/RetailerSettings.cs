namespace Lyke.Application.Configuration;

public class RetailerSettings
{
    public const string SectionName = "Retailer";

    public int MaxImportRows { get; set; } = 10000;
    public int MinAnonymityGroupSize { get; set; } = 5;
    public string DefaultCurrency { get; set; } = "USD";
    public decimal MaxCampaignBudget { get; set; } = 100000m;
}
