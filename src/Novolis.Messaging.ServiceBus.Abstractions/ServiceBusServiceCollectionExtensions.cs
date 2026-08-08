using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Messaging.ServiceBus.Abstractions;

/// <summary>Shared DI helpers for Service Bus options (implementations live in Client / Broker packages).</summary>
public static class ServiceBusServiceCollectionExtensions
{
    /// <summary>Binds <see cref="ServiceBusClientOptions"/> and registers the options instance.</summary>
    public static IServiceCollection AddServiceBusOptions(
        this IServiceCollection services,
        Action<ServiceBusClientOptions>? configure = null)
    {
        var builder = services.AddOptions<ServiceBusClientOptions>();
        if (configure is not null)
            builder.Configure(configure);
        return services;
    }
}
