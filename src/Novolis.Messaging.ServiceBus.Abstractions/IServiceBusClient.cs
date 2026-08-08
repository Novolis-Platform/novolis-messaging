namespace Novolis.Messaging.ServiceBus.Abstractions;

/// <summary>Factory for queue/topic senders and receivers.</summary>
public interface IServiceBusClient : IAsyncDisposable
{
    IQueueSender CreateSender(string queueOrTopicName);

    IQueueReceiver CreateReceiver(string queueName);
}
