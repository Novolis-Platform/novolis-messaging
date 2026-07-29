using Novolis.Messaging.Coordination.InMemory;

namespace Novolis.Messaging.Unit;

public sealed class InMemoryCoordinationTests
{
    [Test]
    public async Task RateLimitCounter_IncrementsWithinWindow()
    {
        var counter = new InMemoryRateLimitCounter();
        var a = await counter.IncrementAsync("k", TimeSpan.FromMinutes(1));
        var b = await counter.IncrementAsync("k", TimeSpan.FromMinutes(1));
        await Assert.That(a).IsEqualTo(1);
        await Assert.That(b).IsEqualTo(2);
    }

    [Test]
    public async Task TokenDenylist_StubAllowsAll()
    {
        var list = new InMemoryTokenDenylist();
        await list.DenyAsync("jti", TimeSpan.FromMinutes(1));
        await Assert.That(await list.IsDeniedAsync("jti")).IsFalse();
    }
}
