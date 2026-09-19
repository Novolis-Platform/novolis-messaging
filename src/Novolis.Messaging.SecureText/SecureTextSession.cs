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

/// <summary>
/// Stateful endpoint session for one pinned peer and one conversation. Hosts must persist
/// <see cref="State"/> after each send or successful receive to preserve replay protection.
/// </summary>
public sealed class SecureTextSession : IDisposable
{
    /// <summary>Largest permitted plaintext character count.</summary>
    public const int MaximumPlaintextCharacters = 2048;

    private static readonly UTF8Encoding Utf8Strict = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private readonly SecureTextConversationId _conversationId;
    private readonly SecureTextDeviceIdentity _localIdentity;
    private readonly SecureTextPublicBundle _localBundle;
    private readonly SecureTextPublicBundle _peerBundle;
    private readonly SecureTextDeviceId _peerDeviceId;
    private readonly SecureTextDeviceId _localDeviceId;
    private byte[] _sessionKey;
    private bool _disposed;

    /// <summary>Creates a session after the caller has completed peer fingerprint confirmation.</summary>
    public SecureTextSession(
        SecureTextDeviceIdentity localIdentity,
        SecureTextPublicBundle localBundle,
        SecureTextPublicBundle peerBundle,
        SecureTextTrustedPeer trustedPeer,
        SecureTextConversationId conversationId,
        SecureTextConversationState? state = null)
    {
        ArgumentNullException.ThrowIfNull(localIdentity);
        ArgumentNullException.ThrowIfNull(localBundle);
        ArgumentNullException.ThrowIfNull(peerBundle);
        ArgumentNullException.ThrowIfNull(trustedPeer);
        if (localIdentity.DeviceId != localBundle.DeviceId)
            throw new CryptographicException("The local public bundle does not match the local device identity.");

        trustedPeer.VerifyBundle(peerBundle);
        if (peerBundle.DeviceId == localIdentity.DeviceId)
            throw new CryptographicException("A secure-text peer must be a different device.");

        _localIdentity = localIdentity;
        _localBundle = localBundle;
        _peerBundle = peerBundle;
        _conversationId = conversationId.Value == Guid.Empty
            ? throw new ArgumentException("A conversation id is required.", nameof(conversationId))
            : conversationId;
        _localDeviceId = SecureTextDeviceId.FromGuid(localIdentity.DeviceId);
        _peerDeviceId = SecureTextDeviceId.FromGuid(peerBundle.DeviceId);
        State = state ?? new SecureTextConversationState();
        _sessionKey = SecureTextKeyAgreement.DeriveSessionKey(
            _localIdentity,
            _localBundle,
            _peerBundle,
            _conversationId.Value,
            State.Snapshot().KeyEpoch);
    }

    /// <summary>Counter and replay state that the host must persist safely.</summary>
    public SecureTextConversationState State { get; }

    /// <summary>Local device id bound to this session.</summary>
    public SecureTextDeviceId LocalDeviceId => _localDeviceId;

    /// <summary>Pinned remote device id bound to this session.</summary>
    public SecureTextDeviceId PeerDeviceId => _peerDeviceId;

    /// <summary>Conversation id bound to this session.</summary>
    public SecureTextConversationId ConversationId => _conversationId;

    /// <summary>Encrypts a text message into an opaque envelope.</summary>
    public SecureTextEnvelope Protect(string text, DateTimeOffset? sentAtUtc = null)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length is < 1 or > MaximumPlaintextCharacters)
            throw new ArgumentOutOfRangeException(nameof(text), $"Text must contain 1 to {MaximumPlaintextCharacters} characters.");

        var plaintext = Utf8Strict.GetBytes(text);
        if (plaintext.Length > SecureTextEnvelopeCodec.MaximumCiphertextBytes)
            throw new ArgumentOutOfRangeException(nameof(text), "The UTF-8 text is too large for a secure-text envelope.");

        try
        {
            var state = State.Snapshot();
            var header = new SecureTextEnvelopeHeader(
                SecureTextProtocol.Version,
                Guid.CreateVersion7(),
                _conversationId,
                _localDeviceId,
                _peerDeviceId,
                state.KeyEpoch,
                State.TakeNextOutboundCounter(),
                (sentAtUtc ?? DateTimeOffset.UtcNow).ToUniversalTime());
            var protectedPayload = SecureTextAead.Seal(
                _sessionKey,
                plaintext,
                SecureTextEnvelopeCodec.GetAssociatedData(header));
            return new SecureTextEnvelope(
                header,
                protectedPayload.Nonce,
                protectedPayload.Ciphertext,
                protectedPayload.AuthenticationTag);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    /// <summary>Authenticates, decrypts, validates, and accepts a fresh inbound envelope.</summary>
    public SecureTextReceivedText Open(SecureTextEnvelope envelope)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(envelope);
        return OpenCore(envelope, inbound: true, trackReplay: true);
    }

    /// <summary>
    /// Decrypts this endpoint's own authenticated sent envelope for local history display.
    /// This method does not alter the inbound replay window.
    /// </summary>
    public SecureTextReceivedText OpenSentHistory(SecureTextEnvelope envelope)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(envelope);
        return OpenCore(envelope, inbound: false, trackReplay: false);
    }

    /// <summary>
    /// Decrypts an authenticated stored envelope for local history display without changing
    /// live replay state. The envelope must have either endpoint as its sender.
    /// </summary>
    public SecureTextReceivedText OpenHistory(SecureTextEnvelope envelope)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(envelope);
        var header = envelope.Header;
        if (header.SenderDeviceId == _peerDeviceId && header.RecipientDeviceId == _localDeviceId)
            return OpenCore(envelope, inbound: true, trackReplay: false);
        if (header.SenderDeviceId == _localDeviceId && header.RecipientDeviceId == _peerDeviceId)
            return OpenCore(envelope, inbound: false, trackReplay: false);

        throw new CryptographicException("The stored envelope device routing does not match this session.");
    }

    /// <summary>Disposes the session and clears its derived symmetric key.</summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        CryptographicOperations.ZeroMemory(_sessionKey);
        _sessionKey = [];
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private SecureTextReceivedText OpenCore(SecureTextEnvelope envelope, bool inbound, bool trackReplay)
    {
        envelope.Validate();
        var header = envelope.Header;
        if (header.ConversationId != _conversationId)
            throw new CryptographicException("The envelope belongs to a different conversation.");
        var expectedSender = inbound ? _peerDeviceId : _localDeviceId;
        var expectedRecipient = inbound ? _localDeviceId : _peerDeviceId;
        if (header.SenderDeviceId != expectedSender || header.RecipientDeviceId != expectedRecipient)
            throw new CryptographicException("The envelope device routing does not match this session.");
        if (header.KeyEpoch != State.Snapshot().KeyEpoch)
            throw new CryptographicException("The envelope key epoch is not active for this session.");

        var ciphertext = new SecureTextCiphertext(
            envelope.ExportNonce(),
            envelope.ExportCiphertext(),
            envelope.ExportAuthenticationTag());
        var plaintext = SecureTextAead.Open(
            _sessionKey,
            ciphertext,
            SecureTextEnvelopeCodec.GetAssociatedData(header));
        try
        {
            var text = Utf8Strict.GetString(plaintext);
            if (text.Length is < 1 or > MaximumPlaintextCharacters)
                throw new CryptographicException("The decrypted text length is invalid.");
            if (trackReplay && !State.TryAcceptInboundCounter(header.Counter))
                throw new CryptographicException("The envelope is a replay, stale, or invalid counter advance.");

            return new SecureTextReceivedText(
                header.MessageId,
                header.ConversationId,
                header.SenderDeviceId,
                header.SentAtUtc,
                text);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }
}
