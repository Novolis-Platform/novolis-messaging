using Azure.Messaging.ServiceBus;
using Novolis.Messaging.ServiceBus.Abstractions;

namespace Novolis.Messaging.ServiceBus.Client;

internal sealed class AzureQueueSender(ServiceBusSender inner) : IQueueSender
{
    public async Task SendAsync<T>(IMessage<T> message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        var sb = ServiceBusMessageMapper.ToServiceBusMessage(message);
        await inner.SendMessageAsync(sb, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync() => inner.DisposeAsync();
}
