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

    /// <summary>Derives a group-scoped pair conversation id for one sender and recipient.</summary>
    public static SecureTextConversationId DeriveForGroupMember(
        SecureTextGroupId groupId,
        SecureTextDeviceId first,
        SecureTextDeviceId second)
    {
        if (groupId.Value == Guid.Empty)
            throw new ArgumentException("A group id is required.", nameof(groupId));
        if (first.Value == Guid.Empty || second.Value == Guid.Empty || first == second)
            throw new ArgumentException("Two distinct device ids are required.");

        var firstBytes = first.Value.ToByteArray(bigEndian: true);
        var secondBytes = second.Value.ToByteArray(bigEndian: true);
        if (firstBytes.AsSpan().SequenceCompareTo(secondBytes) > 0)
            (firstBytes, secondBytes) = (secondBytes, firstBytes);

        Span<byte> material = stackalloc byte["Novolis.SecureText.v1.group-pair"u8.Length + 48];
        "Novolis.SecureText.v1.group-pair"u8.CopyTo(material);
        groupId.Value.ToByteArray(bigEndian: true).CopyTo(material["Novolis.SecureText.v1.group-pair"u8.Length..]);
        firstBytes.CopyTo(material[("Novolis.SecureText.v1.group-pair"u8.Length + 16)..]);
        secondBytes.CopyTo(material[("Novolis.SecureText.v1.group-pair"u8.Length + 32)..]);
        var hash = SHA256.HashData(material);
        var guid = new Guid(hash.AsSpan(0, 16), bigEndian: true);
        return new SecureTextConversationId(guid == Guid.Empty ? new Guid(hash.AsSpan(16, 16), bigEndian: true) : guid);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString("D");
}
