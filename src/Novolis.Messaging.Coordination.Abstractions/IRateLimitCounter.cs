namespace Novolis.Messaging.Coordination.Abstractions;

/// <summary>Fixed-window counter per bucket (e.g. rate limiting); Garnet-backed in multi-instance hosts.</summary>
public interface IRateLimitCounter
{
    /// <summary>Increments the bucket and returns the count after increment.</summary>
    ValueTask<long> IncrementAsync(string bucketKey, TimeSpan windowTtl, CancellationToken cancellationToken = default);
}
