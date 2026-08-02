using Microsoft.Extensions.DependencyInjection;
using Novolis.Messaging.Coordination.Abstractions;
using Novolis.Messaging.Coordination.InMemory;

namespace Novolis.Messaging.Unit;

public sealed class InMemoryCoordinationExtensionsTests
{
    [Test]
    public async Task AddInMemoryCoordination_registers_all_services()
    {
        var services = new ServiceCollection();
        services.AddInMemoryCoordination();
        var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetService<ISessionRealtimePresence>()).IsNotNull();
        await Assert.That(provider.GetService<ISimulationTickLeadership>()).IsNotNull();
        await Assert.That(provider.GetService<ITokenDenylist>()).IsNotNull();
        await Assert.That(provider.GetService<IRateLimitCounter>()).IsNotNull();
    }

    [Test]
    public async Task SessionPresence_tracks_and_untracks_connections()
    {
        var presence = new InMemorySessionRealtimePresence();
        const long session = 42L;

        presence.TrackSessionConnection(session, "conn-a");
        presence.TrackSessionConnection(session, "conn-b");
        await Assert.That(presence.GetSubscriberCount(session)).IsEqualTo(2);

        presence.UntrackSessionConnection(session, "conn-a");
        await Assert.That(presence.GetSubscriberCount(session)).IsEqualTo(1);

        presence.UntrackSessionConnection(session, "conn-b");
        await Assert.That(presence.GetSubscriberCount(session)).IsEqualTo(0);
    }

    [Test]
    public async Task SessionPresence_untrack_unknown_session_is_noop()
    {
        var presence = new InMemorySessionRealtimePresence();
        presence.UntrackSessionConnection(99L, "missing");
        await Assert.That(presence.GetSubscriberCount(99L)).IsEqualTo(0);
    }

    [Test]
    public async Task SimulationTickLeadership_always_acquires()
    {
        var leadership = new InMemorySimulationTickLeadership();
        await Assert.That(await leadership.TryRenewOrAcquireAsync()).IsTrue();
    }

    [Test]
    public async Task RateLimitCounter_resets_after_window()
    {
        var counter = new InMemoryRateLimitCounter();
        var first = await counter.IncrementAsync("burst", TimeSpan.FromMilliseconds(50));
        await Task.Delay(75);
        var second = await counter.IncrementAsync("burst", TimeSpan.FromMilliseconds(50));
        await Assert.That(first).IsEqualTo(1);
        await Assert.That(second).IsEqualTo(1);
    }

    [Test]
    public async Task RateLimitCounter_handles_concurrent_increments()
    {
        var counter = new InMemoryRateLimitCounter();
        var tasks = Enumerable.Range(0, 32)
            .Select(_ => counter.IncrementAsync("hot", TimeSpan.FromMinutes(1)).AsTask());
        var results = await Task.WhenAll(tasks);
        await Assert.That(results.Max()).IsEqualTo(32);
    }
}
