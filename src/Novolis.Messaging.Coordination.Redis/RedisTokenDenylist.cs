using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novolis.Messaging.Coordination.Abstractions;
using StackExchange.Redis;

namespace Novolis.Messaging.Coordination.Redis;

public sealed class RedisTokenDenylist(
    IConnectionMultiplexer mux,
    IOptions<CoordinationHostingOptions> options,
    ILogger<RedisTokenDenylist> logger) : ITokenDenylist
{
    public async ValueTask<bool> IsDeniedAsync(string jti, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(jti))
            return false;
        try
        {
            var db = mux.GetDatabase();
            var key = $"{CoordinationRedisKeys.TokenDeny(options.Value)}:{CoordinationKeyUtility.HashSegment(jti)}";
            var exists = await db.KeyExistsAsync(key).WaitAsync(cancellationToken).ConfigureAwait(false);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Token denylist read failed");
            return false;
        }
    }

    public async ValueTask DenyAsync(string jti, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(jti))
            return;
        try
        {
            var db = mux.GetDatabase();
            var key = $"{CoordinationRedisKeys.TokenDeny(options.Value)}:{CoordinationKeyUtility.HashSegment(jti)}";
            await db.StringSetAsync(key, (RedisValue)"1", ttl).WaitAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Token denylist write failed");
        }
    }
}
