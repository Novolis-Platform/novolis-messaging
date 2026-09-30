using System.Security.Cryptography;
using System.Text;
using Novolis.Security.SecureText;

namespace Novolis.Messaging.SecureText;

/// <summary>Decrypted text and authenticated envelope metadata delivered to an endpoint.</summary>
public sealed record SecureTextReceivedText(
    Guid MessageId,
    SecureTextConversationId ConversationId,
    SecureTextDeviceId SenderDeviceId,
    DateTimeOffset SentAtUtc,
    string Text);
