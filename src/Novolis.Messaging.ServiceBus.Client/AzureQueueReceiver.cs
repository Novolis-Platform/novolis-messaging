using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Novolis.Messaging.ServiceBus.Abstractions;

namespace Novolis.Messaging.ServiceBus.Client;

internal sealed class AzureQueueReceiver(ServiceBusReceiver inner) : IQueueReceiver
{
    private readonly ConcurrentDictionary<string, ServiceBusReceivedMessage> _byLockToken = new(StringComparer.Ordinal);

    public async Task<IMessage<T>?> ReceiveAsync<T>(
        TimeSpan? maxWait = null,
        CancellationToken cancellationToken = default)
    {
        var received = await inner
            .ReceiveMessageAsync(maxWait ?? TimeSpan.FromSeconds(30), cancellationToken)
            .ConfigureAwait(false);

        if (received is null)
            return null;

        _byLockToken[received.LockToken] = received;
        return ServiceBusMessageMapper.FromReceived<T>(received);
    }

    public async Task CompleteAsync<T>(IMessage<T> message, CancellationToken cancellationToken = default)
    {
        var sdk = TakeSdkMessage(message);
        await inner.CompleteMessageAsync(sdk, cancellationToken).ConfigureAwait(false);
    }

    public async Task AbandonAsync<T>(IMessage<T> message, CancellationToken cancellationToken = default)
    {
        var sdk = TakeSdkMessage(message);
        await inner.AbandonMessageAsync(sdk, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task DeadLetterAsync<T>(
        IMessage<T> message,
        string? reason = null,
        string? errorDescription = null,
        CancellationToken cancellationToken = default)
    {
        var sdk = TakeSdkMessage(message);
        await inner
            .DeadLetterMessageAsync(sdk, reason, errorDescription, cancellationToken)
            .ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        _byLockToken.Clear();
        return inner.DisposeAsync();
    }

    private ServiceBusReceivedMessage TakeSdkMessage<T>(IMessage<T> message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var token = message.Advanced.LockToken;
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Message.Advanced.LockToken is required to settle a received message.");

        if (!_byLockToken.TryRemove(token, out var sdk))
            throw new InvalidOperationException("Lock token is unknown to this receiver (message was not received here, or already settled).");

        return sdk;
    }
}
