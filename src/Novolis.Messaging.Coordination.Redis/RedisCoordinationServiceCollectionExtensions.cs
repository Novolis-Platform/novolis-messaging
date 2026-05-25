using Microsoft.Extensions.DependencyInjection;
using Novolis.Messaging.Coordination.Abstractions;

namespace Novolis.Messaging.Coordination.Redis;

public static class RedisCoordinationServiceCollectionExtensions
{
    /// <summary>
    /// Registers Redis-backed coordination. Requires <see cref="IConnectionMultiplexer"/> in DI
    /// (e.g. <c>builder.AddRedisClient("garnet")</c> from Aspire.StackExchange.Redis).
    /// </summary>
    public static IServiceCollection AddRedisCoordinationImplementations(this IServiceCollection services)
    {
        services.AddSingleton<ISessionRealtimePresence, RedisSessionRealtimePresence>();
        services.AddSingleton<ISimulationTickLeadership, RedisSimulationTickLeadership>();
        services.AddSingleton<ITokenDenylist, RedisTokenDenylist>();
        services.AddSingleton<IRateLimitCounter, RedisRateLimitCounter>();
        return services;
    }

    /// <summary>Compatibility alias for hosts that still call the Garnet name.</summary>
    public static IServiceCollection AddGarnetCoordinationImplementations(this IServiceCollection services) =>
        AddRedisCoordinationImplementations(services);
}
