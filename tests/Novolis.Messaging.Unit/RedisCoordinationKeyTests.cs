using Novolis.Messaging.Coordination.Abstractions;
using Novolis.Messaging.Coordination.Redis;

namespace Novolis.Messaging.Unit;

public sealed class RedisCoordinationKeyTests
{
    [Test]
    public async Task NormalizePrefix_uses_default_when_empty()
    {
        await Assert.That(CoordinationRedisKeys.NormalizePrefix("")).IsEqualTo("scr:");
        await Assert.That(CoordinationRedisKeys.NormalizePrefix("   ")).IsEqualTo("scr:");
    }

    [Test]
    public async Task NormalizePrefix_appends_colon_when_missing()
    {
        await Assert.That(CoordinationRedisKeys.NormalizePrefix("game")).IsEqualTo("game:");
        await Assert.That(CoordinationRedisKeys.NormalizePrefix("game:")).IsEqualTo("game:");
    }

    [Test]
    public async Task Key_layouts_include_prefix()
    {
        var options = new CoordinationHostingOptions { KeyPrefix = "demo" };
        await Assert.That(CoordinationRedisKeys.PresencePrefix(options)).IsEqualTo("demo:prt");
        await Assert.That(CoordinationRedisKeys.TickLeader(options)).IsEqualTo("demo:sim:tick-leader");
        await Assert.That(CoordinationRedisKeys.TokenDeny(options)).IsEqualTo("demo:auth:deny:jti");
        await Assert.That(CoordinationRedisKeys.RateLimit(options)).IsEqualTo("demo:rl");
    }

    [Test]
    public async Task HashSegment_is_deterministic_hex()
    {
        var first = CoordinationKeyUtility.HashSegment("token-123");
        var second = CoordinationKeyUtility.HashSegment("token-123");
        var other = CoordinationKeyUtility.HashSegment("token-456");

        await Assert.That(first).IsEqualTo(second);
        await Assert.That(first).IsNotEqualTo(other);
        await Assert.That(first.Length).IsEqualTo(64);
    }
}
