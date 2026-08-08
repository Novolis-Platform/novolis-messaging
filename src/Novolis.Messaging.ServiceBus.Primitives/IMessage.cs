namespace Novolis.Messaging.ServiceBus;

/// <summary>Typed Service Bus message. Prefer <see cref="Payload"/> and identifiers; dig into <see cref="Advanced"/> for broker metadata.</summary>
public interface IMessage<T>
{
    /// <summary>Message id (mapped to Azure <c>MessageId</c> when present).</summary>
    Guid Id { get; }

    /// <summary>Correlation id for tracing related messages.</summary>
    Guid CorrelationId { get; }

    /// <summary>Optional subject / label.</summary>
    string? Subject { get; }

    /// <summary>Deserialized application payload.</summary>
    T Payload { get; }

    /// <summary>Broker and transport metadata (lock token, delivery count, application properties, …).</summary>
    IMessageAdvanced<T> Advanced { get; }
}
