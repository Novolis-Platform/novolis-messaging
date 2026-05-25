using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novolis.Messaging.Coordination.Abstractions;
using StackExchange.Redis;

namespace Novolis.Messaging.Coordination.Redis;

/// <summary>Distributed lease so only one pod advances ticks when enabled.</summary>
public sealed class RedisSimulationTickLeadership(
    IConnectionMultiplexer mux,
    IOptions<CoordinationHostingOptions> options,
    ILogger<RedisSimulationTickLeadership> logger) : ISimulationTickLeadership
{
    private readonly string _instanceId = options.Value.InstanceId
        ?? $"{Environment.MachineName}-{Environment.ProcessId}";

    public async ValueTask<bool> TryRenewOrAcquireAsync(CancellationToken cancellationToken = default)
    {
        var ttl = TimeSpan.FromSeconds(Math.Clamp(options.Value.TickLeadershipLeaseSeconds, 5, 120));
        var leaderKey = CoordinationRedisKeys.TickLeader(options.Value);
        var db = mux.GetDatabase();
        try
        {
            var current = await db.StringGetAsync(leaderKey).WaitAsync(cancellationToken).ConfigureAwait(false);
            if (current.IsNullOrEmpty)
            {
                var acquired =
                    await db.StringSetAsync(leaderKey, _instanceId, ttl, When.NotExists).WaitAsync(cancellationToken)
                        .ConfigureAwait(false);
                if (acquired)
                    return true;
                current = await db.StringGetAsync(leaderKey).WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            if (current == _instanceId)
            {
                await db.KeyExpireAsync(leaderKey, ttl).WaitAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Tick leadership coordination failed");
            return false;
        }
    }
}
