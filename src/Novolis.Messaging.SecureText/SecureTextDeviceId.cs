using System.Buffers.Binary;
using System.Security.Cryptography;
using Novolis.Security.SecureText;

namespace Novolis.Messaging.SecureText;

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
