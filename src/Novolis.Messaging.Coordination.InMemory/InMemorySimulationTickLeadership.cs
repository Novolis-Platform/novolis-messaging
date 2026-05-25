using Novolis.Messaging.Coordination.Abstractions;

namespace Novolis.Messaging.Coordination.InMemory;

/// <summary>Single-process host always holds leadership.</summary>
public sealed class InMemorySimulationTickLeadership : ISimulationTickLeadership
{
    public ValueTask<bool> TryRenewOrAcquireAsync(CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(true);
}
