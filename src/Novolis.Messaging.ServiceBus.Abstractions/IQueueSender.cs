using Novolis.Messaging.ServiceBus;

namespace Novolis.Messaging.ServiceBus.Abstractions;

/// <summary>Sends typed messages to a queue or topic.</summary>
public interface IQueueSender : IAsyncDisposable
{
    Task SendAsync<T>(IMessage<T> message, CancellationToken cancellationToken = default);
}
