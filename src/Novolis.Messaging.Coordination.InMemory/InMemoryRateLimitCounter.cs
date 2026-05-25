using System.Collections.Concurrent;
using Novolis.Messaging.Coordination.Abstractions;

namespace Novolis.Messaging.Coordination.InMemory;

/// <summary>Per-process fixed window; not shared across pods.</summary>
public sealed class InMemoryRateLimitCounter : IRateLimitCounter
{
    private sealed record Bucket(long WindowStartTicks, long Count);

    private readonly ConcurrentDictionary<string, Bucket> _buckets = new();

    public ValueTask<long> IncrementAsync(string bucketKey, TimeSpan windowTtl,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.Ticks;
        var windowTicks = windowTtl.Ticks;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_buckets.TryGetValue(bucketKey, out var bucket))
            {
                var initial = new Bucket(now, 1);
                if (_buckets.TryAdd(bucketKey, initial))
                    return ValueTask.FromResult(1L);
                continue;
            }

            if (now - bucket.WindowStartTicks > windowTicks)
            {
                var fresh = new Bucket(now, 1);
                if (_buckets.TryUpdate(bucketKey, fresh, bucket))
                    return ValueTask.FromResult(1L);
                continue;
            }

            var next = bucket with { Count = bucket.Count + 1 };
            if (_buckets.TryUpdate(bucketKey, next, bucket))
                return ValueTask.FromResult(next.Count);
        }
    }
}
