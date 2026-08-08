using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Options;
using Novolis.Messaging.ServiceBus.Abstractions;
using AbsOptions = Novolis.Messaging.ServiceBus.Abstractions.ServiceBusClientOptions;

namespace Novolis.Messaging.ServiceBus.Client;

/// <summary>Azure SDK administration client (queue create / ensure).</summary>
public sealed class AzureServiceBusAdministration : IServiceBusAdministration
{
    private readonly ServiceBusAdministrationClient _inner;

    internal AzureServiceBusAdministration(ServiceBusAdministrationClient inner) =>
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    public AzureServiceBusAdministration(IOptions<AbsOptions> options)
        : this(CreateAdminClient(options.Value))
    {
    }

    public AzureServiceBusAdministration(AbsOptions options)
        : this(CreateAdminClient(options))
    {
    }

    public static ServiceBusAdministrationClient CreateAdminClient(AbsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ConnectionString);

        var cs = options.ConnectionString.Trim();
        if (options.Provider == ServiceBusProvider.Almost
            && !cs.Contains("UseDevelopmentEmulator=", StringComparison.OrdinalIgnoreCase))
        {
            cs = cs.TrimEnd(';') + ";UseDevelopmentEmulator=true";
        }

        return new ServiceBusAdministrationClient(cs);
    }

    public async Task CreateQueueAsync(string queueName, CancellationToken cancellationToken = default)
    {
        queueName = ServiceBusEntityPath.Queue(queueName);
        await _inner.CreateQueueAsync(queueName, cancellationToken).ConfigureAwait(false);
    }

    public async Task EnsureQueueAsync(string queueName, CancellationToken cancellationToken = default)
    {
        queueName = ServiceBusEntityPath.Queue(queueName);
        if (await _inner.QueueExistsAsync(queueName, cancellationToken).ConfigureAwait(false))
            return;

        try
        {
            await _inner.CreateQueueAsync(queueName, cancellationToken).ConfigureAwait(false);
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
        {
            // raced with another creator
        }
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
