namespace Novolis.Messaging.SecureText;

/// <summary>Persistable state for one direction of a secure-text conversation.</summary>
public sealed record SecureTextConversationStateSnapshot(
    int KeyEpoch,
    long NextOutboundCounter,
    long HighestInboundCounter,
    IReadOnlyList<long> SeenInboundCounters);

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

/// <summary>Mutable sender-counter and bounded inbound-replay state for one peer session.</summary>
public sealed class SecureTextConversationState
{
    /// <summary>Largest permitted jump in an authenticated sender counter.</summary>
    public const long MaximumCounterGap = 1024;

    /// <summary>Number of accepted out-of-order counters retained for replay detection.</summary>
    public const int ReplayWindowSize = 512;

    private readonly Lock _gate = new();
    private long _highestInboundCounter;
    private long _nextOutboundCounter;
    private readonly HashSet<long> _seenInboundCounters;

    /// <summary>Creates fresh state for the first key epoch.</summary>
    public SecureTextConversationState()
        : this(keyEpoch: 1, nextOutboundCounter: 1, highestInboundCounter: 0, seenInboundCounters: [])
    {
    }

    /// <summary>Restores previously persisted counter and replay state.</summary>
    public SecureTextConversationState(SecureTextConversationStateSnapshot snapshot)
        : this(
            snapshot?.KeyEpoch ?? throw new ArgumentNullException(nameof(snapshot)),
            snapshot.NextOutboundCounter,
            snapshot.HighestInboundCounter,
            snapshot.SeenInboundCounters)
    {
    }

    private SecureTextConversationState(
        int keyEpoch,
        long nextOutboundCounter,
        long highestInboundCounter,
        IEnumerable<long> seenInboundCounters)
    {
        if (keyEpoch < 1)
            throw new ArgumentOutOfRangeException(nameof(keyEpoch), "The key epoch must be positive.");
        if (nextOutboundCounter < 1)
            throw new ArgumentOutOfRangeException(nameof(nextOutboundCounter), "The next outbound counter must be positive.");
        if (highestInboundCounter < 0)
            throw new ArgumentOutOfRangeException(nameof(highestInboundCounter), "The highest inbound counter cannot be negative.");
        ArgumentNullException.ThrowIfNull(seenInboundCounters);

        KeyEpoch = keyEpoch;
        _nextOutboundCounter = nextOutboundCounter;
        _highestInboundCounter = highestInboundCounter;
        _seenInboundCounters = new HashSet<long>(seenInboundCounters);
        if (_seenInboundCounters.Any(counter => counter is < 1 || counter > _highestInboundCounter))
            throw new ArgumentException("Persisted inbound counters are invalid.", nameof(seenInboundCounters));
        TrimReplayWindow();
    }

    /// <summary>Current conversation key epoch.</summary>
    public int KeyEpoch { get; private set; }

    /// <summary>Claims the next sender counter. Persist the updated state before sending the envelope.</summary>
    public long TakeNextOutboundCounter()
    {
        lock (_gate)
        {
            if (_nextOutboundCounter == long.MaxValue)
                throw new InvalidOperationException("The sender counter is exhausted; rotate to a new key epoch.");

            return _nextOutboundCounter++;
        }
    }

    /// <summary>Accepts one authenticated inbound counter when it is fresh and within the bounded window.</summary>
    public bool TryAcceptInboundCounter(long counter)
    {
        lock (_gate)
        {
            if (counter < 1)
                return false;
            if (_highestInboundCounter == 0)
            {
                if (counter > MaximumCounterGap)
                    return false;
            }
            else
            {
                if (counter > _highestInboundCounter + MaximumCounterGap)
                    return false;
                if (counter <= _highestInboundCounter - ReplayWindowSize)
                    return false;
            }

            if (!_seenInboundCounters.Add(counter))
                return false;

            if (counter > _highestInboundCounter)
                _highestInboundCounter = counter;
            TrimReplayWindow();
            return true;
        }
    }

    /// <summary>Moves to a verified coordinated key epoch and clears counter state.</summary>
    public void ResetForNewEpoch(int keyEpoch)
    {
        if (keyEpoch < 1)
            throw new ArgumentOutOfRangeException(nameof(keyEpoch), "The key epoch must be positive.");

        lock (_gate)
        {
            if (keyEpoch <= KeyEpoch)
                throw new InvalidOperationException("A new key epoch must increase.");
            KeyEpoch = keyEpoch;
            _nextOutboundCounter = 1;
            _highestInboundCounter = 0;
            _seenInboundCounters.Clear();
        }
    }

    /// <summary>Returns a copy that a host can persist in protected application state.</summary>
    public SecureTextConversationStateSnapshot Snapshot()
    {
        lock (_gate)
        {
            return new SecureTextConversationStateSnapshot(
                KeyEpoch,
                _nextOutboundCounter,
                _highestInboundCounter,
                _seenInboundCounters.Order().ToArray());
        }
    }

    private void TrimReplayWindow()
    {
        var firstRetained = _highestInboundCounter - ReplayWindowSize + 1;
        if (firstRetained <= 1)
            return;

        _seenInboundCounters.RemoveWhere(counter => counter < firstRetained);
    }
}
