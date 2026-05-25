namespace Novolis.Messaging.Coordination.Redis;

internal static class CoordinationRedisKeys
{
    public static string NormalizePrefix(string keyPrefix)
    {
        if (string.IsNullOrWhiteSpace(keyPrefix))
            return "scr:";
        return keyPrefix.EndsWith(':') ? keyPrefix : keyPrefix + ":";
    }

    public static string PresencePrefix(Coordination.Abstractions.CoordinationHostingOptions options) =>
        NormalizePrefix(options.KeyPrefix) + "prt";

    public static string TickLeader(Coordination.Abstractions.CoordinationHostingOptions options) =>
        NormalizePrefix(options.KeyPrefix) + "sim:tick-leader";

    public static string TokenDeny(Coordination.Abstractions.CoordinationHostingOptions options) =>
        NormalizePrefix(options.KeyPrefix) + "auth:deny:jti";

    public static string RateLimit(Coordination.Abstractions.CoordinationHostingOptions options) =>
        NormalizePrefix(options.KeyPrefix) + "rl";
}
