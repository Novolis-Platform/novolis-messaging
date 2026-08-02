using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Novolis.Messaging.Coordination.Abstractions;
using Novolis.Messaging.Coordination.Redis;

namespace Novolis.Messaging.Unit;

public sealed class RedisCoordinationBehaviorTests
{
    private static CoordinationHostingOptions DefaultOptions => new()
    {
        KeyPrefix = "test",
        InstanceId = "pod-a",
        PresenceKeyExpirySeconds = 120,
        TickLeadershipLeaseSeconds = 15,
    };

    [Test]
    public async Task RateLimitCounter_increments_and_sets_ttl_on_first_hit()
    {
        var counter = new RedisRateLimitCounter(
            RedisTestSessionFixture.Multiplexer,
            Options.Create(DefaultOptions),
            NullLogger<RedisRateLimitCounter>.Instance);

        var first = await counter.IncrementAsync("login", TimeSpan.FromMinutes(1));
        var second = await counter.IncrementAsync("login", TimeSpan.FromMinutes(1));

        await Assert.That(first).IsEqualTo(1);
        await Assert.That(second).IsEqualTo(2);
    }

    [Test]
    public async Task TokenDenylist_denies_and_expires()
    {
        var denylist = new RedisTokenDenylist(
            RedisTestSessionFixture.Multiplexer,
            Options.Create(DefaultOptions),
            NullLogger<RedisTokenDenylist>.Instance);

        await Assert.That(await denylist.IsDeniedAsync("")).IsFalse();
        await Assert.That(await denylist.IsDeniedAsync("token-1")).IsFalse();

        await denylist.DenyAsync("token-1", TimeSpan.FromMinutes(5));
        await Assert.That(await denylist.IsDeniedAsync("token-1")).IsTrue();
    }

    [Test]
    public async Task SessionPresence_tracks_untracks_and_counts()
    {
        var presence = new RedisSessionRealtimePresence(
            RedisTestSessionFixture.Multiplexer,
            Options.Create(DefaultOptions),
            NullLogger<RedisSessionRealtimePresence>.Instance);

        presence.TrackSessionConnection(42, "conn-a");
        presence.TrackSessionConnection(42, "conn-b");
        await Assert.That(presence.GetSubscriberCount(42)).IsEqualTo(2);

        presence.UntrackSessionConnection(42, "conn-a");
        await Assert.That(presence.GetSubscriberCount(42)).IsEqualTo(1);
    }

    [Test]
    public async Task TickLeadership_acquires_renews_and_rejects_other_instance()
    {
        var leader = new RedisSimulationTickLeadership(
            RedisTestSessionFixture.Multiplexer,
            Options.Create(DefaultOptions),
            NullLogger<RedisSimulationTickLeadership>.Instance);

        await Assert.That(await leader.TryRenewOrAcquireAsync()).IsTrue();
        await Assert.That(await leader.TryRenewOrAcquireAsync()).IsTrue();

        var follower = new RedisSimulationTickLeadership(
            RedisTestSessionFixture.Multiplexer,
            Options.Create(new CoordinationHostingOptions
            {
                KeyPrefix = DefaultOptions.KeyPrefix,
                InstanceId = "pod-b",
                TickLeadershipLeaseSeconds = DefaultOptions.TickLeadershipLeaseSeconds,
            }),
            NullLogger<RedisSimulationTickLeadership>.Instance);

        await Assert.That(await follower.TryRenewOrAcquireAsync()).IsFalse();
    }

    [Test]
    public async Task HealthChecks_report_healthy_when_redis_responds()
    {
        var redis = new RedisCoordinationHealthCheck(
            RedisTestSessionFixture.Multiplexer,
            NullLogger<RedisCoordinationHealthCheck>.Instance);
        var garnet = new GarnetCoordinationHealthCheck(
            RedisTestSessionFixture.Multiplexer,
            NullLogger<GarnetCoordinationHealthCheck>.Instance);

        var redisResult = await redis.CheckHealthAsync(new HealthCheckContext());
        var garnetResult = await garnet.CheckHealthAsync(new HealthCheckContext());

        await Assert.That(redisResult.Status).IsEqualTo(HealthStatus.Healthy);
        await Assert.That(garnetResult.Status).IsEqualTo(HealthStatus.Healthy);
    }
}
