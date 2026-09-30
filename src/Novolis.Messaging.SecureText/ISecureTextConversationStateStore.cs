namespace Novolis.Messaging.SecureText;

/// <summary>Storage port for counters and replay state that must survive a host restart.</summary>
public interface ISecureTextConversationStateStore
{
    /// <summary>Loads state for one local and remote device pair.</summary>
    Task<SecureTextConversationStateSnapshot?> LoadAsync(
        SecureTextConversationId conversationId,
        SecureTextDeviceId localDeviceId,
        SecureTextDeviceId remoteDeviceId,
        CancellationToken cancellationToken = default);

    /// <summary>Persists state immediately after a counter is used or accepted.</summary>
    Task StoreAsync(
        SecureTextConversationId conversationId,
        SecureTextDeviceId localDeviceId,
        SecureTextDeviceId remoteDeviceId,
        SecureTextConversationStateSnapshot snapshot,
        CancellationToken cancellationToken = default);
}
