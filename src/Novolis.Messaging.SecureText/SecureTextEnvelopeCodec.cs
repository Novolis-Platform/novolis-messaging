using System.Buffers.Binary;
using System.Security.Cryptography;
using Novolis.Security.SecureText;

namespace Novolis.Messaging.SecureText;

/// <summary>Canonical binary codec and associated-data builder for secure-text envelopes.</summary>
public static class SecureTextEnvelopeCodec
{
    /// <summary>Largest encrypted text payload accepted by the v1 protocol.</summary>
    public const int MaximumCiphertextBytes = 8192;

    /// <summary>Returns the authenticated bytes for a clear envelope header.</summary>
    public static byte[] GetAssociatedData(SecureTextEnvelopeHeader header)
    {
        ArgumentNullException.ThrowIfNull(header);
        header.Validate();

        using var stream = new MemoryStream();
        WriteHeader(stream, header);
        return stream.ToArray();
    }

    /// <summary>Serializes an envelope to canonical binary for relay or durable storage.</summary>
    public static byte[] Serialize(SecureTextEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        envelope.Validate();

        var nonce = envelope.ExportNonce();
        var ciphertext = envelope.ExportCiphertext();
        var tag = envelope.ExportAuthenticationTag();
        using var stream = new MemoryStream();
        WriteHeader(stream, envelope.Header);
        stream.Write(nonce);
        WriteInt32(stream, ciphertext.Length);
        stream.Write(ciphertext);
        stream.Write(tag);
        return stream.ToArray();
    }

    /// <summary>Deserializes a canonical envelope and rejects malformed or trailing data.</summary>
    public static SecureTextEnvelope Deserialize(ReadOnlySpan<byte> payload)
    {
        if (payload.IsEmpty || payload.Length > GetMaximumSerializedBytes())
            throw new InvalidDataException("The secure-text envelope length is invalid.");

        using var stream = new MemoryStream(payload.ToArray(), writable: false);
        var header = ReadHeader(stream);
        var nonce = ReadExact(stream, SecureTextProtocol.NonceBytes);
        var cipherLength = ReadInt32(stream);
        if (cipherLength is < 1 or > MaximumCiphertextBytes)
            throw new InvalidDataException("The secure-text ciphertext length is invalid.");

        var ciphertext = ReadExact(stream, cipherLength);
        var tag = ReadExact(stream, SecureTextProtocol.AuthenticationTagBytes);
        if (stream.Position != stream.Length)
            throw new InvalidDataException("The secure-text envelope has trailing data.");

        return new SecureTextEnvelope(header, nonce, ciphertext, tag);
    }

    /// <summary>Largest valid serialized v1 envelope size.</summary>
    public static int GetMaximumSerializedBytes() =>
        sizeof(int) +
        (sizeof(byte) * 16 * 4) +
        sizeof(int) +
        sizeof(long) +
        sizeof(long) +
        SecureTextProtocol.NonceBytes +
        sizeof(int) +
        MaximumCiphertextBytes +
        SecureTextProtocol.AuthenticationTagBytes;

    private static SecureTextEnvelopeHeader ReadHeader(Stream stream)
    {
        var protocolVersion = ReadInt32(stream);
        var messageId = ReadGuid(stream);
        var conversationId = SecureTextConversationId.FromGuid(ReadGuid(stream));
        var sender = SecureTextDeviceId.FromGuid(ReadGuid(stream));
        var recipient = SecureTextDeviceId.FromGuid(ReadGuid(stream));
        var keyEpoch = ReadInt32(stream);
        var counter = ReadInt64(stream);
        var sentAtUnixMilliseconds = ReadInt64(stream);
        var header = new SecureTextEnvelopeHeader(
            protocolVersion,
            messageId,
            conversationId,
            sender,
            recipient,
            keyEpoch,
            counter,
            DateTimeOffset.FromUnixTimeMilliseconds(sentAtUnixMilliseconds));
        header.Validate();
        return header;
    }

    private static void WriteHeader(Stream stream, SecureTextEnvelopeHeader header)
    {
        WriteInt32(stream, header.ProtocolVersion);
        WriteGuid(stream, header.MessageId);
        WriteGuid(stream, header.ConversationId.Value);
        WriteGuid(stream, header.SenderDeviceId.Value);
        WriteGuid(stream, header.RecipientDeviceId.Value);
        WriteInt32(stream, header.KeyEpoch);
        WriteInt64(stream, header.Counter);
        WriteInt64(stream, header.SentAtUtc.ToUniversalTime().ToUnixTimeMilliseconds());
    }

    private static byte[] ReadExact(Stream stream, int length)
    {
        var bytes = new byte[length];
        var offset = 0;
        while (offset < bytes.Length)
        {
            var read = stream.Read(bytes, offset, bytes.Length - offset);
            if (read == 0)
                throw new EndOfStreamException("The secure-text envelope is truncated.");
            offset += read;
        }

        return bytes;
    }

    private static Guid ReadGuid(Stream stream) => new(ReadExact(stream, 16), bigEndian: true);

    private static int ReadInt32(Stream stream) => BinaryPrimitives.ReadInt32BigEndian(ReadExact(stream, sizeof(int)));

    private static long ReadInt64(Stream stream) => BinaryPrimitives.ReadInt64BigEndian(ReadExact(stream, sizeof(long)));

    private static void WriteGuid(Stream stream, Guid value) => stream.Write(value.ToByteArray(bigEndian: true));

    private static void WriteInt32(Stream stream, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        stream.Write(bytes);
    }

    private static void WriteInt64(Stream stream, long value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(bytes, value);
        stream.Write(bytes);
    }
}
