namespace Lyke.Infrastructure.Configuration;

public class AzureBlobSettings
{
    public const string SectionName = "AzureBlob";

    public required string ConnectionString { get; set; }
    public string ContainerName { get; set; } = "media";
}
