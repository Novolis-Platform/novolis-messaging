using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Novolis.Messaging.ServiceBus.Abstractions;

namespace Novolis.Messaging.ServiceBus.Broker.Almost;

internal sealed class AlmostServiceBusBrokerHostedService(AlmostServiceBusBroker broker) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => broker.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => broker.StopAsync();
}
