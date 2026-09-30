using System.Buffers.Binary;
using System.Security.Cryptography;
using Novolis.Security.SecureText;

namespace Novolis.Messaging.SecureText;

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
