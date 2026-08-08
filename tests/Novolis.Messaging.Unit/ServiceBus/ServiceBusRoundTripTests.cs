using Novolis.Messaging.ServiceBus.Broker.Almost;
using Novolis.Messaging.ServiceBus.Client;

namespace Novolis.Messaging.Unit.ServiceBus;

public sealed class ServiceBusRoundTripTests
{
    [Test]
    public async Task Send_receive_complete_round_trip()
    {
        await using var broker = new AlmostServiceBusBroker();
        await broker.StartAsync();

        var options = broker.CreateClientOptions();
        await using var admin = new AzureServiceBusAdministration(options);
        var queueName = $"q-{Guid.NewGuid():N}";
        await admin.EnsureQueueAsync(queueName);

        await using var client = new AzureServiceBusClient(options);
        await using var sender = client.CreateSender(queueName);
        await using var receiver = client.CreateReceiver(queueName);

        var sent = new Novolis.Messaging.ServiceBus.Message<SamplePayload>(
            new SamplePayload(42, "novolis"),
            correlationId: Guid.NewGuid(),
            subject: "round-trip");

        await sender.SendAsync(sent);

        var received = await receiver.ReceiveAsync<SamplePayload>(TimeSpan.FromSeconds(10));
        await Assert.That(received).IsNotNull();
        await Assert.That(received!.Payload.Id).IsEqualTo(42);
        await Assert.That(received.Payload.Name).IsEqualTo("novolis");
        await Assert.That(received.Subject).IsEqualTo("round-trip");
        await Assert.That(received.CorrelationId).IsEqualTo(sent.CorrelationId);
        await Assert.That(received.Advanced.LockToken).IsNotNull();
        await Assert.That(received.Advanced.DeliveryCount).IsEqualTo(1);

        await receiver.CompleteAsync(received);
    }

    [Test]
    public async Task Message_advanced_is_progressive()
    {
        var message = new Novolis.Messaging.ServiceBus.Message<string>("ping");
        await Assert.That(message.Payload).IsEqualTo("ping");
        await Assert.That(message.Advanced.LockToken).IsNull();
        await Assert.That(message.Advanced.ApplicationProperties.Count).IsEqualTo(0);

        var withSession = message with
        {
            Advanced = message.Advanced with { SessionId = "s1", ContentType = "text/plain" },
        };
        await Assert.That(withSession.Advanced.SessionId).IsEqualTo("s1");
        await Assert.That(withSession.Payload).IsEqualTo("ping");
    }

    private sealed record SamplePayload(int Id, string Name);
}
