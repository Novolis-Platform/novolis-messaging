using System.Collections.Concurrent;
using Novolis.Messaging.Coordination.Abstractions;

namespace Novolis.Messaging.Coordination.InMemory;

/// <summary>Thread-safe ref count per session and connection (same semantics as legacy hub tracking).</summary>
public sealed class InMemorySessionRealtimePresence : ISessionRealtimePresence
{
    private readonly ConcurrentDictionary<long, ConcurrentDictionary<string, byte>> _sessionConnections = new();

    public void TrackSessionConnection(long sessionTicks, string connectionId)
    {
        var inner = _sessionConnections.GetOrAdd(sessionTicks, _ => new ConcurrentDictionary<string, byte>());
        inner.TryAdd(connectionId, 0);
    }

    public void UntrackSessionConnection(long sessionTicks, string connectionId)
    {
        if (!_sessionConnections.TryGetValue(sessionTicks, out var inner))
            return;
        inner.TryRemove(connectionId, out _);
        if (inner.IsEmpty)
            _sessionConnections.TryRemove(sessionTicks, out _);
    }

    public int GetSubscriberCount(long sessionTicks) =>
        _sessionConnections.TryGetValue(sessionTicks, out var inner) ? inner.Count : 0;
}
