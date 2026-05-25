namespace Novolis.Messaging.Coordination.Abstractions;

/// <summary>Hosts pick <see cref="Mode"/> InMemory (single process / tests) vs Garnet (multi-pod).</summary>
public sealed class CoordinationHostingOptions
{
    public const string SectionName = "Coordination";

    /// <summary>InMemory or Garnet (case-insensitive).</summary>
    public string Mode { get; set; } = "InMemory";

    /// <summary>Aspire resource / connection string name for StackExchange.Redis client (Garnet RESP).</summary>
    public string RedisConnectionName { get; set; } = "garnet";

    /// <summary>When true, only the pod holding the distributed lease publishes simulation ticks.</summary>
    public bool RequireDistributedTickLeadership { get; set; }

    /// <summary>Lease TTL applied by Garnet leadership (renewed each tick loop).</summary>
    public int TickLeadershipLeaseSeconds { get; set; } = 15;

    /// <summary>Stable id for this instance when using Garnet leases (defaults to machine + process id at runtime).</summary>
    public string? InstanceId { get; set; }

    /// <summary>TTL for per-connection realtime presence keys in Garnet.</summary>
    public int PresenceKeyExpirySeconds { get; set; } = 120;

    /// <summary>Redis key prefix (e.g. <c>scr:</c>).</summary>
    public string KeyPrefix { get; set; } = "scr:";
}
