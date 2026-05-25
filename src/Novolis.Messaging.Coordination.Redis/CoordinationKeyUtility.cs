using System.Security.Cryptography;
using System.Text;

namespace Novolis.Messaging.Coordination.Redis;

internal static class CoordinationKeyUtility
{
    public static string HashSegment(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
