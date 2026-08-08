using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace Novolis.Messaging.ServiceBus.Client;

internal static class ServiceBusMessageMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static ServiceBusMessage ToServiceBusMessage<T>(IMessage<T> message)
    {
        var body = EncodePayload(message.Payload, message.Advanced);
        var sb = new ServiceBusMessage(body)
        {
            MessageId = message.Id.ToString("D"),
            Subject = message.Subject,
        };

        if (message.CorrelationId != Guid.Empty)
            sb.CorrelationId = message.CorrelationId.ToString("D");

        var advanced = message.Advanced;
        if (!string.IsNullOrEmpty(advanced.SessionId))
            sb.SessionId = advanced.SessionId;
        if (!string.IsNullOrEmpty(advanced.ReplyTo))
            sb.ReplyTo = advanced.ReplyTo;
        if (!string.IsNullOrEmpty(advanced.ContentType))
            sb.ContentType = advanced.ContentType;
        if (!string.IsNullOrEmpty(advanced.PartitionKey))
            sb.PartitionKey = advanced.PartitionKey;

        foreach (var (key, value) in advanced.ApplicationProperties)
            sb.ApplicationProperties[key] = value;

        return sb;
    }

    public static Message<T> FromReceived<T>(ServiceBusReceivedMessage received)
    {
        var payload = DecodePayload<T>(received.Body, received.ContentType);
        var id = Guid.TryParse(received.MessageId, out var parsedId) ? parsedId : Guid.NewGuid();
        var correlation = Guid.TryParse(received.CorrelationId, out var parsedCorr)
            ? parsedCorr
            : Guid.Empty;

        var props = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var pair in received.ApplicationProperties)
            props[pair.Key] = pair.Value ?? "";

        var advanced = new MessageAdvanced<T>
        {
            SessionId = received.SessionId,
            ReplyTo = received.ReplyTo,
            ContentType = received.ContentType,
            DeliveryCount = received.DeliveryCount,
            EnqueuedTime = received.EnqueuedTime,
            LockedUntil = received.LockedUntil,
            LockToken = received.LockToken,
            PartitionKey = received.PartitionKey,
            ApplicationProperties = props,
            RawBody = received.Body.ToMemory(),
        };

        return new Message<T>(payload, id, correlation, received.Subject, advanced);
    }

    private static BinaryData EncodePayload<T>(T payload, IMessageAdvanced<T> advanced)
    {
        if (advanced.RawBody is { } raw)
            return BinaryData.FromBytes(raw);

        if (payload is null)
            return BinaryData.FromBytes(ReadOnlyMemory<byte>.Empty);

        if (payload is BinaryData binaryData)
            return binaryData;

        if (payload is byte[] bytes)
            return BinaryData.FromBytes(bytes);

        if (payload is ReadOnlyMemory<byte> rom)
            return BinaryData.FromBytes(rom);

        if (payload is string s)
            return BinaryData.FromString(s);

        return BinaryData.FromBytes(JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions));
    }

    private static T DecodePayload<T>(BinaryData body, string? contentType)
    {
        if (typeof(T) == typeof(BinaryData))
            return (T)(object)body;

        if (typeof(T) == typeof(byte[]))
            return (T)(object)body.ToArray();

        if (typeof(T) == typeof(ReadOnlyMemory<byte>))
            return (T)(object)body.ToMemory();

        if (typeof(T) == typeof(string))
            return (T)(object)body.ToString();

        if (body.ToMemory().IsEmpty)
        {
            if (default(T) is null)
                return default!;
            throw new InvalidOperationException($"Empty Service Bus body cannot deserialize to {typeof(T)}.");
        }

        _ = contentType;
        return JsonSerializer.Deserialize<T>(body.ToMemory().Span, JsonOptions)
               ?? throw new InvalidOperationException($"JSON body deserialized to null for {typeof(T)}.");
    }
}
