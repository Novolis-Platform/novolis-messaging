using Novolis.Messaging.ServiceBus.Client;
using Novolis.Messaging.ServiceBus.Broker.Almost;
using Novolis.Messaging.ServiceBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Novolis.Messaging.Unit.ServiceBus;

public sealed class ServiceBusCoverageGapTests
{
    [Test]
    public async Task EntityPath_queue_topic_subscription()
    {
        await Assert.That(Novolis.Messaging.ServiceBus.ServiceBusEntityPath.Queue("  orders  ")).IsEqualTo("orders");
        await Assert.That(Novolis.Messaging.ServiceBus.ServiceBusEntityPath.Topic(" events ")).IsEqualTo("events");
        await Assert.That(Novolis.Messaging.ServiceBus.ServiceBusEntityPath.Subscription("events", "sub-a"))
            .IsEqualTo("events/subscriptions/sub-a");

        await Assert.That(() => Novolis.Messaging.ServiceBus.ServiceBusEntityPath.Queue(" ")).Throws<ArgumentException>();
        await Assert.That(() => Novolis.Messaging.ServiceBus.ServiceBusEntityPath.Topic("")).Throws<ArgumentException>();
        await Assert.That(() => Novolis.Messaging.ServiceBus.ServiceBusEntityPath.Subscription("t", " ")).Throws<ArgumentException>();
    }

    [Test]
    public async Task AddServiceBusOptions_with_and_without_configure()
    {
        var plain = new ServiceCollection();
        plain.AddServiceBusOptions();
        await using var plainSp = plain.BuildServiceProvider();
        var plainOpts = plainSp.GetRequiredService<IOptions<ServiceBusClientOptions>>().Value;
        await Assert.That(string.IsNullOrEmpty(plainOpts.ConnectionString)).IsTrue();

        var configured = new ServiceCollection();
        configured.AddServiceBusOptions(o =>
        {
            o.ConnectionString = "Endpoint=sb://x/;SharedAccessKeyName=a;SharedAccessKey=b";
            o.Provider = ServiceBusProvider.Azure;
        });
        await using var configuredSp = configured.BuildServiceProvider();
        var opts = configuredSp.GetRequiredService<IOptions<ServiceBusClientOptions>>().Value;
        await Assert.That(opts.Provider).IsEqualTo(ServiceBusProvider.Azure);
        await Assert.That(opts.ConnectionString).Contains("sb://x/");
    }

    [Test]
    public async Task AddServiceBusClient_and_Almost_broker_host_wires_options()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAlmostServiceBusBroker();
        services.AddServiceBusClient();

        await using var provider = services.BuildServiceProvider();
        var host = provider.GetRequiredService<IHostedService>();
        await host.StartAsync(CancellationToken.None);
        try
        {
            var broker = provider.GetRequiredService<AlmostServiceBusBroker>();
            await Assert.That(broker.IsStarted).IsTrue();
            await Assert.That(broker.AmqpConnectionString).IsNotNull();
            await Assert.That(broker.Namespace).IsNotNull();

            var options = provider.GetRequiredService<IOptions<ServiceBusClientOptions>>().Value;
            await Assert.That(options.Provider).IsEqualTo(ServiceBusProvider.Almost);
            await Assert.That(options.ConnectionString).IsEqualTo(broker.ConnectionString);
            await Assert.That(options.PublicPort).IsEqualTo(broker.PublicPort);

            var client = provider.GetRequiredService<IServiceBusClient>();
            var admin = provider.GetRequiredService<IServiceBusAdministration>();
            var queue = $"q-{Guid.NewGuid():N}";
            await admin.CreateQueueAsync(queue);

            await using var sender = client.CreateSender(queue);
            await using var receiver = client.CreateReceiver(queue);

            var advanced = new Novolis.Messaging.ServiceBus.MessageAdvanced<string>
            {
                SessionId = "sess-1",
                ReplyTo = "reply-q",
                ContentType = "text/plain",
                PartitionKey = "sess-1",
                ApplicationProperties = new Dictionary<string, object> { ["k"] = "v" },
            };
            var sent = new Novolis.Messaging.ServiceBus.Message<string>(
                "payload",
                correlationId: Guid.NewGuid(),
                subject: "subj",
                advanced: advanced);
            await sender.SendAsync(sent);

            var received = await receiver.ReceiveAsync<string>(TimeSpan.FromSeconds(10));
            await Assert.That(received).IsNotNull();
            await Assert.That(received!.Payload).IsEqualTo("payload");
            await Assert.That(received.Advanced.SessionId).IsEqualTo("sess-1");
            await Assert.That(received.Advanced.ReplyTo).IsEqualTo("reply-q");
            await Assert.That(received.Advanced.ContentType).IsEqualTo("text/plain");
            await Assert.That(received.Advanced.PartitionKey).IsEqualTo("sess-1");
            await Assert.That(received.Advanced.ApplicationProperties["k"]).IsEqualTo("v");

            await receiver.AbandonAsync(received);
            var again = await receiver.ReceiveAsync<string>(TimeSpan.FromSeconds(10));
            await Assert.That(again).IsNotNull();
            await receiver.DeadLetterAsync(again!, reason: "test", errorDescription: "coverage");
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Test]
    public async Task Mapper_payload_variants_and_receive_timeout_null()
    {
        await using var broker = new AlmostServiceBusBroker();
        await broker.StartAsync();
        await broker.StartAsync();
        await broker.StopAsync();
        await broker.StopAsync();
        await broker.StartAsync();

        var options = broker.CreateClientOptions();
        await using var admin = new AzureServiceBusAdministration(Options.Create(options));
        var queue = $"q-{Guid.NewGuid():N}";
        await admin.EnsureQueueAsync(queue);
        await admin.EnsureQueueAsync(queue);

        await using var client = new AzureServiceBusClient(Options.Create(options));
        await using var sender = client.CreateSender(queue);
        await using var receiver = client.CreateReceiver(queue);

        await sender.SendAsync(new Novolis.Messaging.ServiceBus.Message<byte[]>([1, 2, 3], subject: "bytes"));
        var asBytes = await receiver.ReceiveAsync<byte[]>(TimeSpan.FromSeconds(10));
        await Assert.That(asBytes!.Payload).IsEquivalentTo(new byte[] { 1, 2, 3 });
        await receiver.CompleteAsync(asBytes);

        await sender.SendAsync(new Novolis.Messaging.ServiceBus.Message<string>("raw-string", subject: "str"));
        var asString = await receiver.ReceiveAsync<string>(TimeSpan.FromSeconds(10));
        await Assert.That(asString!.Payload).IsEqualTo("raw-string");
        await receiver.CompleteAsync(asString);

        var rawBody = new Novolis.Messaging.ServiceBus.MessageAdvanced<string> { RawBody = "via-raw"u8.ToArray() };
        await sender.SendAsync(new Novolis.Messaging.ServiceBus.Message<string>("ignored", advanced: rawBody));
        var fromRaw = await receiver.ReceiveAsync<string>(TimeSpan.FromSeconds(10));
        await Assert.That(fromRaw!.Payload).IsEqualTo("via-raw");
        await receiver.CompleteAsync(fromRaw);

        var timedOut = await receiver.ReceiveAsync<string>(TimeSpan.FromMilliseconds(50));
        await Assert.That(timedOut).IsNull();

        await Assert.That(() => receiver.CompleteAsync(new Novolis.Messaging.ServiceBus.Message<string>("x")))
            .Throws<InvalidOperationException>();

        // Unknown lock token after receive+complete already settled.
        await sender.SendAsync(new Novolis.Messaging.ServiceBus.Message<string>("again"));
        var once = await receiver.ReceiveAsync<string>(TimeSpan.FromSeconds(10));
        await receiver.CompleteAsync(once!);
        await Assert.That(() => receiver.CompleteAsync(once!))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Mapper_binarydata_rom_null_and_json_empty_body()
    {
        await using var broker = new AlmostServiceBusBroker();
        await broker.StartAsync();
        var options = broker.CreateClientOptions();
        // Emulator flag already present — cover early return in EnsureEmulatorFlag.
        options.ConnectionString = options.ConnectionString.TrimEnd(';') + ";UseDevelopmentEmulator=true";
        await using var admin = new AzureServiceBusAdministration(options);
        var queue = $"q-{Guid.NewGuid():N}";
        await admin.EnsureQueueAsync(queue);

        await using var client = new AzureServiceBusClient(options);
        await using var sender = client.CreateSender(queue);
        await using var receiver = client.CreateReceiver(queue);

        await sender.SendAsync(new Novolis.Messaging.ServiceBus.Message<BinaryData>(
            BinaryData.FromBytes([9, 8]), subject: "bd"));
        var asBd = await receiver.ReceiveAsync<BinaryData>(TimeSpan.FromSeconds(10));
        await Assert.That(asBd!.Payload.ToArray()).IsEquivalentTo(new byte[] { 9, 8 });
        await receiver.CompleteAsync(asBd);

        ReadOnlyMemory<byte> rom = new byte[] { 4, 5, 6 };
        await sender.SendAsync(new Novolis.Messaging.ServiceBus.Message<ReadOnlyMemory<byte>>(rom, subject: "rom"));
        var asRom = await receiver.ReceiveAsync<ReadOnlyMemory<byte>>(TimeSpan.FromSeconds(10));
        await Assert.That(asRom!.Payload.ToArray()).IsEquivalentTo(new byte[] { 4, 5, 6 });
        await receiver.CompleteAsync(asRom);

        // Encode null payload → empty body; decode as byte[] (string ToString breaks on empty BinaryData).
        string? nullPayload = null;
        await sender.SendAsync(new Novolis.Messaging.ServiceBus.Message<string?>(nullPayload, subject: "nil"));
        var asEmpty = await receiver.ReceiveAsync<byte[]>(TimeSpan.FromSeconds(10));
        await Assert.That(asEmpty!.Payload).IsEmpty();
        await receiver.CompleteAsync(asEmpty);

        // Default wait path (maxWait null).
        await sender.SendAsync(new Novolis.Messaging.ServiceBus.Message<string>("wait-default"));
        var withDefaultWait = await receiver.ReceiveAsync<string>();
        await Assert.That(withDefaultWait).IsNotNull();
        await receiver.CompleteAsync(withDefaultWait!);
    }

    [Test]
    public async Task CreateSdkClient_azure_provider_skips_custom_endpoint()
    {
        var options = new ServiceBusClientOptions
        {
            Provider = ServiceBusProvider.Azure,
            ConnectionString =
                "Endpoint=sb://example.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        };

        await using var client = new AzureServiceBusClient(options);
        await Assert.That(client).IsNotNull();
    }
}