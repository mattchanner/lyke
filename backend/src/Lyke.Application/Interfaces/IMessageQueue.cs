namespace Lyke.Application.Interfaces;

public interface IMessageQueue<T>
{
    Task EnqueueAsync(T message, CancellationToken cancellationToken = default);
}
