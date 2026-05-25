using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novolis.Messaging.Coordination.Abstractions;
using StackExchange.Redis;

namespace Novolis.Messaging.Coordination.Redis;

/// <summary>Presence via TTL string keys <c>{prefix}prt:{sessionTicks}:{connHash}</c>.</summary>
public sealed class RedisSessionRealtimePresence(
    IConnectionMultiplexer mux,
    IOptions<CoordinationHostingOptions> options,
    ILogger<RedisSessionRealtimePresence> logger) : ISessionRealtimePresence
{
    public void TrackSessionConnection(long sessionTicks, string connectionId)
    {
        var ttl = TimeSpan.FromSeconds(Math.Clamp(options.Value.PresenceKeyExpirySeconds, 30, 3600));
        var db = mux.GetDatabase();
        var key = BuildPresenceKey(sessionTicks, connectionId);
        try
        {
            _ = db.StringSet(key, (RedisValue)"1", ttl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to track presence for session {SessionTicks}", sessionTicks);
        }
    }

    public void UntrackSessionConnection(long sessionTicks, string connectionId)
    {
        var db = mux.GetDatabase();
        var key = BuildPresenceKey(sessionTicks, connectionId);
        try
        {
            _ = db.KeyDelete(key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to untrack presence for session {SessionTicks}", sessionTicks);
        }
    }

    public int GetSubscriberCount(long sessionTicks)
    {
        var prefix = CoordinationRedisKeys.PresencePrefix(options.Value);
        var pattern = $"{prefix}:{sessionTicks}:*";
        try
        {
            var total = 0;
            foreach (var endpoint in mux.GetEndPoints())
            {
                var server = mux.GetServer(endpoint);
                if (!server.IsConnected)
                    continue;
                foreach (var _ in server.Keys(pattern: pattern))
                    total++;
            }

            return total;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to count presence for session {SessionTicks}", sessionTicks);
            return 0;
        }
    }

    private RedisKey BuildPresenceKey(long sessionTicks, string connectionId)
    {
        var prefix = CoordinationRedisKeys.PresencePrefix(options.Value);
        var hash = CoordinationKeyUtility.HashSegment(connectionId);
        return $"{prefix}:{sessionTicks}:{hash}";
    }
}
