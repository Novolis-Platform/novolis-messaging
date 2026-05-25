using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novolis.Messaging.Coordination.Abstractions;
using StackExchange.Redis;

namespace Novolis.Messaging.Coordination.Redis;

public sealed class RedisRateLimitCounter(
    IConnectionMultiplexer mux,
    IOptions<CoordinationHostingOptions> options,
    ILogger<RedisRateLimitCounter> logger) : IRateLimitCounter
{
    public async ValueTask<long> IncrementAsync(string bucketKey, TimeSpan windowTtl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = mux.GetDatabase();
            var key = $"{CoordinationRedisKeys.RateLimit(options.Value)}:{CoordinationKeyUtility.HashSegment(bucketKey)}";
            var count = await db.StringIncrementAsync(key).WaitAsync(cancellationToken).ConfigureAwait(false);
            if (count == 1)
                await db.KeyExpireAsync(key, windowTtl).WaitAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Rate limit increment failed for bucket");
            return 0;
        }
    }
}
