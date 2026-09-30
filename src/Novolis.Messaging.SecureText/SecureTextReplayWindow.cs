namespace Novolis.Messaging.SecureText;

/// <summary>Persistable state for one direction of a secure-text conversation.</summary>
public sealed record SecureTextConversationStateSnapshot(
    int KeyEpoch,
    long NextOutboundCounter,
    long HighestInboundCounter,
    IReadOnlyList<long> SeenInboundCounters);
