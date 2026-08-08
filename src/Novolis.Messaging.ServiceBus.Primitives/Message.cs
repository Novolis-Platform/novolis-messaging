namespace Novolis.Messaging.ServiceBus;

/// <summary>
/// Typed Service Bus envelope. Construct with a payload for send; receivers fill <see cref="Advanced"/>.
/// </summary>
public sealed record Message<T> : IMessage<T>
{
    public Message(
        T payload,
        Guid id = default,
        Guid correlationId = default,
        string? subject = null,
        MessageAdvanced<T>? advanced = null)
    {
        Payload = payload;
        Id = id == default ? Guid.NewGuid() : id;
        CorrelationId = correlationId;
        Subject = subject;
        Advanced = advanced ?? MessageAdvanced<T>.Empty;
    }

    public T Payload { get; init; }

    public Guid Id { get; init; }

    public Guid CorrelationId { get; init; }

    public string? Subject { get; init; }

    /// <summary>Broker metadata. Use for lock tokens, sessions, application properties.</summary>
    public MessageAdvanced<T> Advanced { get; init; }

    IMessageAdvanced<T> IMessage<T>.Advanced => Advanced;
}
