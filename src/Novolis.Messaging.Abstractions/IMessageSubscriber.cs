namespace Novolis.Messaging;

/// <summary>Subscribes to typed messages.</summary>
public interface IMessageSubscriber<T>
{
    /// <summary>Subscribes a handler to incoming messages.</summary>
    Task SubscribeAsync(Func<Message<T>, CancellationToken, Task> handler, CancellationToken cancellationToken = default);
}
