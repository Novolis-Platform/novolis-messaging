namespace Novolis.Messaging.ServiceBus;

/// <summary>
/// Progressive-disclosure surface for Service Bus metadata.
/// Reach via <see cref="IMessage{T}.Advanced"/> — not required for everyday send/receive of payloads.
/// </summary>
public interface IMessageAdvanced<T>
{
    string? SessionId { get; }

    string? ReplyTo { get; }

    string? ContentType { get; }

    int DeliveryCount { get; }

    DateTimeOffset? EnqueuedTime { get; }

    DateTimeOffset? LockedUntil { get; }

    /// <summary>Peek-lock token required to complete, abandon, or dead-letter a received message.</summary>
    string? LockToken { get; }

    string? PartitionKey { get; }

    IReadOnlyDictionary<string, object> ApplicationProperties { get; }

    /// <summary>Raw body when payload deserialization was bypassed or failed open.</summary>
    ReadOnlyMemory<byte>? RawBody { get; }
}
