using System.Buffers.Binary;
using System.Security.Cryptography;
using Novolis.Security.SecureText;

namespace Novolis.Messaging.SecureText;

/// <summary>Stable identifier for a protected two-party text conversation.</summary>
public readonly record struct SecureTextConversationId(Guid Value)
{
    /// <summary>Creates a new conversation id.</summary>
    public static SecureTextConversationId New() => new(Guid.CreateVersion7());

    /// <summary>Creates a validated conversation id.</summary>
    public static SecureTextConversationId FromGuid(Guid value) =>
        value == Guid.Empty
            ? throw new ArgumentException("A conversation id is required.", nameof(value))
            : new SecureTextConversationId(value);

    /// <summary>Derives a stable conversation id for exactly two device ids.</summary>
    public static SecureTextConversationId DeriveForPair(SecureTextDeviceId first, SecureTextDeviceId second)
    {
        if (first.Value == Guid.Empty || second.Value == Guid.Empty || first == second)
            throw new ArgumentException("Two distinct device ids are required.");

        var firstBytes = first.Value.ToByteArray(bigEndian: true);
        var secondBytes = second.Value.ToByteArray(bigEndian: true);
        if (firstBytes.AsSpan().SequenceCompareTo(secondBytes) > 0)
            (firstBytes, secondBytes) = (secondBytes, firstBytes);

        Span<byte> material = stackalloc byte["Novolis.SecureText.v1.pair"u8.Length + 32];
        "Novolis.SecureText.v1.pair"u8.CopyTo(material);
        firstBytes.CopyTo(material["Novolis.SecureText.v1.pair"u8.Length..]);
        secondBytes.CopyTo(material[("Novolis.SecureText.v1.pair"u8.Length + 16)..]);
        var hash = SHA256.HashData(material);
        var guid = new Guid(hash.AsSpan(0, 16), bigEndian: true);
        return new SecureTextConversationId(guid == Guid.Empty ? new Guid(hash.AsSpan(16, 16), bigEndian: true) : guid);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString("D");
}

/// <summary>Stable identifier for one secure-text device.</summary>
public readonly record struct SecureTextDeviceId(Guid Value)
{
    /// <summary>Creates a validated device id.</summary>
    public static SecureTextDeviceId FromGuid(Guid value) =>
        value == Guid.Empty
            ? throw new ArgumentException("A device id is required.", nameof(value))
            : new SecureTextDeviceId(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString("D");
}

/// <summary>Authenticated, clear routing header for a secure-text envelope.</summary>
public sealed record SecureTextEnvelopeHeader(
    int ProtocolVersion,
    Guid MessageId,
    SecureTextConversationId ConversationId,
    SecureTextDeviceId SenderDeviceId,
    SecureTextDeviceId RecipientDeviceId,
    int KeyEpoch,
    long Counter,
    DateTimeOffset SentAtUtc)
{
    /// <summary>Validates all clear envelope fields before they are authenticated or routed.</summary>
    public void Validate()
    {
        if (ProtocolVersion != SecureTextProtocol.Version)
            throw new NotSupportedException($"Secure-text protocol version {ProtocolVersion} is not supported.");
        if (MessageId == Guid.Empty)
            throw new InvalidDataException("A message id is required.");
        if (ConversationId.Value == Guid.Empty)
            throw new InvalidDataException("A conversation id is required.");
        if (SenderDeviceId.Value == Guid.Empty || RecipientDeviceId.Value == Guid.Empty)
            throw new InvalidDataException("Both device ids are required.");
        if (SenderDeviceId == RecipientDeviceId)
            throw new InvalidDataException("Sender and recipient devices must differ.");
        if (KeyEpoch < 1)
            throw new InvalidDataException("The key epoch must be positive.");
        if (Counter < 1)
            throw new InvalidDataException("The sender counter must be positive.");
        if (SentAtUtc == default)
            throw new InvalidDataException("A sent timestamp is required.");
    }
}

/// <summary>Opaque end-to-end protected message suitable for any delivery transport.</summary>
public sealed class SecureTextEnvelope
{
    private readonly byte[] _authenticationTag;
    private readonly byte[] _ciphertext;
    private readonly byte[] _nonce;

    /// <summary>Creates a validated encrypted envelope.</summary>
    public SecureTextEnvelope(
        SecureTextEnvelopeHeader header,
        ReadOnlySpan<byte> nonce,
        ReadOnlySpan<byte> ciphertext,
        ReadOnlySpan<byte> authenticationTag)
    {
        ArgumentNullException.ThrowIfNull(header);
        Header = header;
        _nonce = nonce.ToArray();
        _ciphertext = ciphertext.ToArray();
        _authenticationTag = authenticationTag.ToArray();
        Validate();
    }

    /// <summary>Authenticated routing and replay metadata.</summary>
    public SecureTextEnvelopeHeader Header { get; }

    /// <summary>Returns the unique AES-GCM nonce.</summary>
    public byte[] ExportNonce() => _nonce.ToArray();

    /// <summary>Returns the opaque encrypted text.</summary>
    public byte[] ExportCiphertext() => _ciphertext.ToArray();

    /// <summary>Returns the AES-GCM authentication tag.</summary>
    public byte[] ExportAuthenticationTag() => _authenticationTag.ToArray();

    /// <summary>Validates envelope limits without decrypting its payload.</summary>
    public void Validate()
    {
        Header.Validate();
        if (_nonce.Length != SecureTextProtocol.NonceBytes)
            throw new InvalidDataException("The AES-GCM nonce length is invalid.");
        if (_ciphertext.Length is < 1 or > SecureTextEnvelopeCodec.MaximumCiphertextBytes)
            throw new InvalidDataException("The ciphertext length is invalid.");
        if (_authenticationTag.Length != SecureTextProtocol.AuthenticationTagBytes)
            throw new InvalidDataException("The AES-GCM authentication tag length is invalid.");
    }
}

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
