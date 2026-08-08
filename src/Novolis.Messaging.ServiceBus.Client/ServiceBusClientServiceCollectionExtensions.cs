using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Novolis.Messaging.ServiceBus.Abstractions;
using AbsOptions = Novolis.Messaging.ServiceBus.Abstractions.ServiceBusClientOptions;

namespace Novolis.Messaging.ServiceBus.Client;

public static class ServiceBusClientServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IServiceBusClient"/> and <see cref="IServiceBusAdministration"/> using Azure.Messaging.ServiceBus.</summary>
    public static IServiceCollection AddServiceBusClient(
        this IServiceCollection services,
        Action<AbsOptions>? configure = null)
    {
        services.AddServiceBusOptions(configure);
        services.AddSingleton<IServiceBusClient>(sp =>
            new AzureServiceBusClient(sp.GetRequiredService<IOptions<AbsOptions>>()));
        services.AddSingleton<IServiceBusAdministration>(sp =>
            new AzureServiceBusAdministration(sp.GetRequiredService<IOptions<AbsOptions>>()));
        return services;
    }
}
