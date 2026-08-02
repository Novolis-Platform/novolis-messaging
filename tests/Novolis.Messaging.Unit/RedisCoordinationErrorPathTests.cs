using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Novolis.Messaging.Coordination.Abstractions;
using Novolis.Messaging.Coordination.Redis;
using StackExchange.Redis;

namespace Novolis.Messaging.Unit;

public sealed class RedisCoordinationErrorPathTests
{
    private static CoordinationHostingOptions DefaultOptions => new()
    {
        KeyPrefix = "test",
        InstanceId = "pod-a",
        PresenceKeyExpirySeconds = 120,
        TickLeadershipLeaseSeconds = 15,
    };

    [Test]
    public async Task RateLimitCounter_returns_zero_when_redis_unavailable()
    {
        await using var broken = await BrokenMultiplexer.CreateAsync();
        var counter = new RedisRateLimitCounter(
            broken.Multiplexer,
            Options.Create(DefaultOptions),
            NullLogger<RedisRateLimitCounter>.Instance);

        await Assert.That(await counter.IncrementAsync("bucket", TimeSpan.FromMinutes(1))).IsEqualTo(0);
    }

    [Test]
    public async Task TokenDenylist_fails_open_when_redis_unavailable()
    {
        await using var broken = await BrokenMultiplexer.CreateAsync();
        var denylist = new RedisTokenDenylist(
            broken.Multiplexer,
            Options.Create(DefaultOptions),
            NullLogger<RedisTokenDenylist>.Instance);

        await Assert.That(await denylist.IsDeniedAsync("token")).IsFalse();
        await denylist.DenyAsync("token", TimeSpan.FromMinutes(1));
    }

    [Test]
    public async Task SessionPresence_swallows_redis_failures()
    {
        await using var broken = await BrokenMultiplexer.CreateAsync();
        var presence = new RedisSessionRealtimePresence(
            broken.Multiplexer,
            Options.Create(DefaultOptions),
            NullLogger<RedisSessionRealtimePresence>.Instance);

        presence.TrackSessionConnection(1, "conn");
        presence.UntrackSessionConnection(1, "conn");
        await Assert.That(presence.GetSubscriberCount(1)).IsEqualTo(0);
    }

    [Test]
    public async Task TickLeadership_returns_false_when_redis_unavailable()
    {
        await using var broken = await BrokenMultiplexer.CreateAsync();
        var leadership = new RedisSimulationTickLeadership(
            broken.Multiplexer,
            Options.Create(DefaultOptions),
            NullLogger<RedisSimulationTickLeadership>.Instance);

        await Assert.That(await leadership.TryRenewOrAcquireAsync()).IsFalse();
    }

    [Test]
    public async Task HealthChecks_report_unhealthy_when_redis_unavailable()
    {
        await using var broken = await BrokenMultiplexer.CreateAsync();
        var redis = new RedisCoordinationHealthCheck(broken.Multiplexer, NullLogger<RedisCoordinationHealthCheck>.Instance);
        var garnet = new GarnetCoordinationHealthCheck(broken.Multiplexer, NullLogger<GarnetCoordinationHealthCheck>.Instance);

        var redisResult = await redis.CheckHealthAsync(new HealthCheckContext());
        var garnetResult = await garnet.CheckHealthAsync(new HealthCheckContext());

        await Assert.That(redisResult.Status).IsEqualTo(HealthStatus.Unhealthy);
        await Assert.That(garnetResult.Status).IsEqualTo(HealthStatus.Unhealthy);
    }

    private sealed class BrokenMultiplexer : IAsyncDisposable
    {
        private readonly IConnectionMultiplexer _multiplexer;

        private BrokenMultiplexer(IConnectionMultiplexer multiplexer) => _multiplexer = multiplexer;

        public IConnectionMultiplexer Multiplexer => _multiplexer;

        public static async Task<BrokenMultiplexer> CreateAsync()
        {
            var mux = await ConnectionMultiplexer.ConnectAsync(RedisTestSessionFixture.ConnectionString);
            await mux.DisposeAsync();
            return new BrokenMultiplexer(mux);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
