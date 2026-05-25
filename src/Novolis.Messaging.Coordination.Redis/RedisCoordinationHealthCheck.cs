using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Novolis.Messaging.Coordination.Redis;

/// <summary>Verifies RESP connectivity to Garnet (or any Redis-compatible endpoint).</summary>
public sealed class RedisCoordinationHealthCheck(IConnectionMultiplexer mux, ILogger<RedisCoordinationHealthCheck> logger)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await mux.GetDatabase().PingAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis coordination health check failed");
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }

}

/// <summary>Compatibility type name for existing health check registration.</summary>
public sealed class GarnetCoordinationHealthCheck(IConnectionMultiplexer mux, ILogger<GarnetCoordinationHealthCheck> logger)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await mux.GetDatabase().PingAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Garnet coordination health check failed");
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}
