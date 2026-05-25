namespace Novolis.Messaging.Coordination.Abstractions;

/// <summary>Tracks SignalR subscribers per session across pods (Garnet) or in-process (InMemory).</summary>
public interface ISessionRealtimePresence
{
    /// <summary>Registers one subscriber for session ticks (per distinct <paramref name="connectionId"/>).</summary>
    void TrackSessionConnection(long sessionTicks, string connectionId);

    void UntrackSessionConnection(long sessionTicks, string connectionId);

    int GetSubscriberCount(long sessionTicks);
}
