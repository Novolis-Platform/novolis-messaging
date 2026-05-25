namespace Novolis.Messaging.Coordination.Abstractions;

/// <summary>Ephemeral JWT identifier revocation (Garnet); InMemory is a no-op.</summary>
public interface ITokenDenylist
{
    ValueTask<bool> IsDeniedAsync(string jti, CancellationToken cancellationToken = default);

    ValueTask DenyAsync(string jti, TimeSpan ttl, CancellationToken cancellationToken = default);
}
