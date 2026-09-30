using System.Buffers.Binary;
using System.Security.Cryptography;
using Novolis.Security.SecureText;

namespace Novolis.Messaging.SecureText;

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
