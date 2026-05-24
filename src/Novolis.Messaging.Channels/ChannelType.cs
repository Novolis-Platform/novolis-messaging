namespace Novolis.Messaging.Channels;

/// <summary>
/// Selects bounded versus unbounded channel creation.
/// </summary>
public enum ChannelType
{
    /// <summary>
    /// Unbounded channel has no limit on the number of items it can store.
    /// </summary>
    Unbounded,

    /// <summary>
    /// Bounded channel has a limit on the number of items it can store.
    /// </summary>
    Bounded
}
