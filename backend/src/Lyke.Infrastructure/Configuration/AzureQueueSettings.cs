namespace Lyke.Infrastructure.Configuration;

public class AzureQueueSettings
{
    public const string SectionName = "AzureQueue";
    public string MediaProcessingQueueName { get; set; } = "media-processing";
}
