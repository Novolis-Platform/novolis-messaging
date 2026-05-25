using Novolis.Messaging.Coordination.Abstractions;

namespace Novolis.Messaging.Coordination.InMemory;

public sealed class InMemoryTokenDenylist : ITokenDenylist
{
    public ValueTask<bool> IsDeniedAsync(string jti, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(false);

    public ValueTask DenyAsync(string jti, TimeSpan ttl, CancellationToken cancellationToken = default) =>
        ValueTask.CompletedTask;
}
