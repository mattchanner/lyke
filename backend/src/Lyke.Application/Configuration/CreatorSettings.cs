namespace Lyke.Application.Configuration;

public class CreatorSettings
{
    public const string SectionName = "Creator";

    public int MaxDraftPosts { get; set; } = 10;
    public int MaxMediaPerPost { get; set; } = 10;
    public int MaxProductsPerPost { get; set; } = 20;
    public decimal MinPayoutThreshold { get; set; } = 50.00m;
    public string DefaultCurrency { get; set; } = "USD";
}
