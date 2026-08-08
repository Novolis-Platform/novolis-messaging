namespace Novolis.Messaging.ServiceBus;

/// <summary>Default implementation of <see cref="IMessageAdvanced{T}"/>.</summary>
public sealed record MessageAdvanced<T> : IMessageAdvanced<T>
{
    public static MessageAdvanced<T> Empty { get; } = new();

    public string? SessionId { get; init; }

    public string? ReplyTo { get; init; }

    public string? ContentType { get; init; }

    public int DeliveryCount { get; init; }

    public DateTimeOffset? EnqueuedTime { get; init; }

    public DateTimeOffset? LockedUntil { get; init; }

    public string? LockToken { get; init; }

    public string? PartitionKey { get; init; }

    public IReadOnlyDictionary<string, object> ApplicationProperties { get; init; } =
        new Dictionary<string, object>();

    public ReadOnlyMemory<byte>? RawBody { get; init; }
}
