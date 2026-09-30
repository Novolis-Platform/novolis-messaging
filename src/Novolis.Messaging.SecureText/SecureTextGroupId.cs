using System.Buffers.Binary;
using System.Security.Cryptography;
using Novolis.Security.SecureText;

namespace Novolis.Messaging.SecureText;

/// <summary>Stable identifier for a secure-text group membership epoch.</summary>
public readonly record struct SecureTextGroupId(Guid Value)
{
    /// <summary>Creates a new group membership epoch.</summary>
    public static SecureTextGroupId New() => new(Guid.CreateVersion7());

    /// <summary>Creates a validated group id.</summary>
    public static SecureTextGroupId FromGuid(Guid value) =>
        value == Guid.Empty
            ? throw new ArgumentException("A group id is required.", nameof(value))
            : new SecureTextGroupId(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString("D");
}
