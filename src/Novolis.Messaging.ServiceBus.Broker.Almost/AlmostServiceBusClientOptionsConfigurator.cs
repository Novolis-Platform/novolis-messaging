using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Novolis.Messaging.ServiceBus.Abstractions;

namespace Novolis.Messaging.ServiceBus.Broker.Almost;

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
