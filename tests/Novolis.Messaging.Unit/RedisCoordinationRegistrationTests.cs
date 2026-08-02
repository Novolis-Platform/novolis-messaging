using Microsoft.Extensions.DependencyInjection;
using Novolis.Messaging.Coordination.Abstractions;
using Novolis.Messaging.Coordination.Redis;

namespace Novolis.Messaging.Unit;

public sealed class RedisCoordinationRegistrationTests
{
    [Test]
    public async Task AddRedisCoordinationImplementations_registers_singletons()
    {
        var services = new ServiceCollection();
        services.AddRedisCoordinationImplementations();

        await Assert.That(services.Single(d => d.ServiceType == typeof(ISessionRealtimePresence)).ImplementationType)
            .IsEqualTo(typeof(RedisSessionRealtimePresence));
        await Assert.That(services.Single(d => d.ServiceType == typeof(ISimulationTickLeadership)).ImplementationType)
            .IsEqualTo(typeof(RedisSimulationTickLeadership));
        await Assert.That(services.Single(d => d.ServiceType == typeof(ITokenDenylist)).ImplementationType)
            .IsEqualTo(typeof(RedisTokenDenylist));
        await Assert.That(services.Single(d => d.ServiceType == typeof(IRateLimitCounter)).ImplementationType)
            .IsEqualTo(typeof(RedisRateLimitCounter));
    }

    [Test]
    public async Task AddGarnetCoordinationImplementations_is_alias()
    {
        var services = new ServiceCollection();
        services.AddGarnetCoordinationImplementations();
        await Assert.That(services.Count(d => d.ServiceType == typeof(ITokenDenylist))).IsEqualTo(1);
    }
}
