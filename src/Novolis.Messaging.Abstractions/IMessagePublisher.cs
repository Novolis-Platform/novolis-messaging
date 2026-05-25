namespace Novolis.Messaging;

/// <summary>Publishes typed messages.</summary>
public interface IMessagePublisher<T>
{
    /// <summary>Publishes a message.</summary>
    Task PublishAsync(Message<T> message, CancellationToken cancellationToken = default);
}
