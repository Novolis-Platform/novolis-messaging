using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using Novolis.Messaging.ServiceBus.Abstractions;
using AbsOptions = Novolis.Messaging.ServiceBus.Abstractions.ServiceBusClientOptions;

namespace Novolis.Messaging.ServiceBus.Client;

/// <summary>Azure SDK-backed <see cref="IServiceBusClient"/>.</summary>
public sealed class AzureServiceBusClient : IServiceBusClient
{
    private readonly ServiceBusClient _inner;

    internal AzureServiceBusClient(ServiceBusClient inner) =>
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    public AzureServiceBusClient(IOptions<AbsOptions> options)
        : this(CreateSdkClient(options.Value))
    {
    }

    public AzureServiceBusClient(AbsOptions options)
        : this(CreateSdkClient(options))
    {
    }

    public static ServiceBusClient CreateSdkClient(AbsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ConnectionString);

        var connectionString = EnsureEmulatorFlag(options);
        var sdkOptions = new Azure.Messaging.ServiceBus.ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpTcp,
        };

        if (options.Provider == ServiceBusProvider.Almost || options.PublicPort is > 0)
        {
            var port = options.PublicPort ?? 5672;
            sdkOptions.CustomEndpointAddress = new Uri($"sb://localhost:{port}");
        }

        return new ServiceBusClient(connectionString, sdkOptions);
    }

    public IQueueSender CreateSender(string queueOrTopicName) =>
        new AzureQueueSender(_inner.CreateSender(ServiceBusEntityPath.Queue(queueOrTopicName)));

    public IQueueReceiver CreateReceiver(string queueName) =>
        new AzureQueueReceiver(_inner.CreateReceiver(ServiceBusEntityPath.Queue(queueName)));

    public ValueTask DisposeAsync() => _inner.DisposeAsync();

    private static string EnsureEmulatorFlag(AbsOptions options)
    {
        var cs = options.ConnectionString.Trim();
        if (options.Provider != ServiceBusProvider.Almost)
            return cs;

        if (cs.Contains("UseDevelopmentEmulator=", StringComparison.OrdinalIgnoreCase))
            return cs;

        return cs.TrimEnd(';') + ";UseDevelopmentEmulator=true";
    }
}
