using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Novolis.Messaging.ServiceBus.Abstractions;

namespace Novolis.Messaging.ServiceBus.Broker.Almost;

public static class AlmostServiceBusServiceCollectionExtensions
{
    /// <summary>
    /// Registers an <see cref="AlmostServiceBusBroker"/> hosted service and wires
    /// <see cref="ServiceBusClientOptions"/> to its connection string after start.
    /// Pair with <c>AddServiceBusClient</c> from the Client package.
    /// </summary>
    public static IServiceCollection AddAlmostServiceBusBroker(this IServiceCollection services)
    {
        services.AddServiceBusOptions();
        services.AddSingleton<AlmostServiceBusBroker>();
        services.AddHostedService<AlmostServiceBusBrokerHostedService>();
        services.AddSingleton<IConfigureOptions<ServiceBusClientOptions>, AlmostServiceBusClientOptionsConfigurator>();
        return services;
    }
}

internal sealed class AlmostServiceBusClientOptionsConfigurator(AlmostServiceBusBroker broker)
    : IConfigureOptions<ServiceBusClientOptions>
{
    public void Configure(ServiceBusClientOptions options)
    {
        options.Provider = ServiceBusProvider.Almost;
        if (broker.IsStarted)
        {
            options.ConnectionString = broker.ConnectionString;
            options.PublicPort = broker.PublicPort;
        }
    }
}

internal sealed class AlmostServiceBusBrokerHostedService(AlmostServiceBusBroker broker) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => broker.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => broker.StopAsync();
}
