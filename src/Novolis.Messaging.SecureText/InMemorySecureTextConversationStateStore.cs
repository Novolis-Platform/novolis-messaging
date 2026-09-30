namespace Novolis.Messaging.SecureText;

/// <summary>Process-local conversation-state store for tests and short-lived hosts.</summary>
public sealed class InMemorySecureTextConversationStateStore : ISecureTextConversationStateStore
{
    private readonly Dictionary<ConversationStateKey, SecureTextConversationStateSnapshot> _states = [];
    private readonly Lock _gate = new();

    /// <inheritdoc />
    public Task<SecureTextConversationStateSnapshot?> LoadAsync(
        SecureTextConversationId conversationId,
        SecureTextDeviceId localDeviceId,
        SecureTextDeviceId remoteDeviceId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = new ConversationStateKey(conversationId, localDeviceId, remoteDeviceId);
        lock (_gate)
            return Task.FromResult(
                _states.TryGetValue(key, out var snapshot)
                    ? Clone(snapshot)
                    : null);
    }

    /// <inheritdoc />
    public Task StoreAsync(
        SecureTextConversationId conversationId,
        SecureTextDeviceId localDeviceId,
        SecureTextDeviceId remoteDeviceId,
        SecureTextConversationStateSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(snapshot);
        var key = new ConversationStateKey(conversationId, localDeviceId, remoteDeviceId);
        lock (_gate)
            _states[key] = Clone(snapshot);
        return Task.CompletedTask;
    }

    private static SecureTextConversationStateSnapshot Clone(SecureTextConversationStateSnapshot snapshot) =>
        snapshot with { SeenInboundCounters = snapshot.SeenInboundCounters.ToArray() };

    private readonly record struct ConversationStateKey(
        SecureTextConversationId ConversationId,
        SecureTextDeviceId LocalDeviceId,
        SecureTextDeviceId RemoteDeviceId);
}
