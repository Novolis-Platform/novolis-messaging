namespace Novolis.Messaging;

/// <summary>Envelope for in-process messages.</summary>
public class Message<T>
{
    /// <summary>Unique message id.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>Correlation id for tracing flows.</summary>
    public Guid CorrelationId { get; set; } = Guid.Empty;

    /// <summary>Payload.</summary>
    public T? Payload { get; init; }
}
