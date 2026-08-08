namespace Novolis.Messaging.ServiceBus.Abstractions;

/// <summary>Management operations (queue CRUD for v1).</summary>
public interface IServiceBusAdministration : IAsyncDisposable
{
    Task CreateQueueAsync(string queueName, CancellationToken cancellationToken = default);

    Task EnsureQueueAsync(string queueName, CancellationToken cancellationToken = default);
}
