namespace Novolis.Messaging.Coordination.Abstractions;

/// <summary>Optional multi-pod gate: only instances that renew this lease should advance the shared simulation ticker.</summary>
public interface ISimulationTickLeadership
{
    ValueTask<bool> TryRenewOrAcquireAsync(CancellationToken cancellationToken = default);
}
