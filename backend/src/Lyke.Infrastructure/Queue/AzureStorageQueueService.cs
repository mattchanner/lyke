using System.Text.Json;
using Azure.Storage.Queues;
using Lyke.Application.Interfaces;
using Lyke.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;

namespace Lyke.Infrastructure.Queue;

public class AzureStorageQueueService<T> : IMessageQueue<T>
{
    private readonly QueueClient _queueClient;
    private readonly ILogger<AzureStorageQueueService<T>> _logger;

    public AzureStorageQueueService(
        AzureBlobSettings blobSettings,
        AzureQueueSettings queueSettings,
        ILogger<AzureStorageQueueService<T>> logger)
    {
        _logger = logger;

        var options = new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.None
        };

        _queueClient = new QueueClient(
            blobSettings.ConnectionString,
            queueSettings.MediaProcessingQueueName,
            options);
    }

    public async Task EnqueueAsync(T message, CancellationToken cancellationToken = default)
    {
        await _queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(message);
        await _queueClient.SendMessageAsync(json, cancellationToken);

        _logger.LogDebug(
            "Enqueued message of type {Type} to queue {Queue}",
            typeof(T).Name,
            _queueClient.Name);
    }
}
