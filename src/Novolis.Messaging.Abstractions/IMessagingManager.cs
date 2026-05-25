namespace Novolis.Messaging;

/// <summary>Creates channel-backed topics and subscriptions.</summary>
public interface IMessagingManager
{
    /// <summary>Creates a topic channel for <typeparamref name="T"/>.</summary>
    Task CreateTopicAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>Creates a subscription for <typeparamref name="T"/>.</summary>
    Task CreateSubscriptionAsync<T>(CancellationToken cancellationToken = default);
}
