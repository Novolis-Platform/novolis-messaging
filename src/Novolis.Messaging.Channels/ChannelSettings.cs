using System.Threading.Channels;

namespace Novolis.Messaging.Channels;

/// <summary>
/// Options applied when creating a <see cref="System.Threading.Channels.Channel{T}"/> via DI.
/// </summary>
public class ChannelSettings
{
    /// <summary>When true, only one reader is allowed.</summary>
    public bool SingleReader { get; set; } = true;

    /// <summary>When true, only one writer is allowed.</summary>
    public bool SingleWriter { get; set; } = true;

    /// <summary>Capacity when using a bounded channel.</summary>
    public int BoundedCapacity { get; set; } = 100;

    /// <summary>Behavior when a bounded channel is full.</summary>
    public BoundedChannelFullMode BoundedFullMode { get; set; } = BoundedChannelFullMode.Wait;
}
