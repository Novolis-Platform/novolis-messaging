using System.Threading.Channels;

namespace Novolis.Messaging.Channels;

/// <summary>Channel-backed <see cref="IMessagePublisher{T}"/>.</summary>
public sealed class ChannelMessagePublisher<T>(ChannelWriter<Message<T>> writer) : IMessagePublisher<T>
{
    /// <inheritdoc />
    public async Task PublishAsync(Message<T> message, CancellationToken cancellationToken = default) =>
        await writer.WriteAsync(message, cancellationToken).ConfigureAwait(false);
}
