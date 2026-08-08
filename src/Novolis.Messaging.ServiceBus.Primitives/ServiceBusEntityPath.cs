namespace Novolis.Messaging.ServiceBus;

/// <summary>Helpers for queue / topic / subscription path segments.</summary>
public static class ServiceBusEntityPath
{
    public static string Queue(string queueName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        return queueName.Trim();
    }

    public static string Topic(string topicName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicName);
        return topicName.Trim();
    }

    public static string Subscription(string topicName, string subscriptionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicName);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);
        return $"{topicName.Trim()}/subscriptions/{subscriptionName.Trim()}";
    }
}
