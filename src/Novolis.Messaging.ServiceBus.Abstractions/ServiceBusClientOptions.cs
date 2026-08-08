namespace Novolis.Messaging.ServiceBus.Abstractions;

/// <summary>Connection and transport options for <see cref="IServiceBusClient"/>.</summary>
public sealed class ServiceBusClientOptions
{
    public const string SectionName = "Novolis:Messaging:ServiceBus";

    /// <summary>Azure or Almost provider.</summary>
    public ServiceBusProvider Provider { get; set; } = ServiceBusProvider.Azure;

    /// <summary>Full connection string (include <c>UseDevelopmentEmulator=true</c> for Almost).</summary>
    public string ConnectionString { get; set; } = "";

    /// <summary>
    /// Public AMQP port for Almost / local emulator. When set, the client uses
    /// <c>CustomEndpointAddress</c> <c>sb://localhost:{port}</c>.
    /// </summary>
    public int? PublicPort { get; set; }
}
