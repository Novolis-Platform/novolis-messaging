using Novolis.Messaging.ServiceBus;

namespace Novolis.Messaging.ServiceBus.Abstractions;

/// <summary>Receives and settles typed peek-lock messages from a queue.</summary>
public interface IQueueReceiver : IAsyncDisposable
{
    Task<IMessage<T>?> ReceiveAsync<T>(TimeSpan? maxWait = null, CancellationToken cancellationToken = default);

    Task CompleteAsync<T>(IMessage<T> message, CancellationToken cancellationToken = default);

    Task AbandonAsync<T>(IMessage<T> message, CancellationToken cancellationToken = default);

    Task DeadLetterAsync<T>(
        IMessage<T> message,
        string? reason = null,
        string? errorDescription = null,
        CancellationToken cancellationToken = default);
}
