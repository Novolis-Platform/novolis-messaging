using AlmostServiceBus.TestHost;
using Novolis.Messaging.ServiceBus.Abstractions;

namespace Novolis.Messaging.ServiceBus.Broker.Almost;

/// <summary>In-process AlmostServiceBus host with per-instance namespace isolation.</summary>
public sealed class AlmostServiceBusBroker : IAsyncDisposable
{
    private readonly ServiceBusEmulatorFixture _fixture = new();
    private int _started;

    public string ConnectionString => _fixture.ConnectionString;

    public string AmqpConnectionString => _fixture.AmqpConnectionString;

    public int PublicPort => _fixture.PublicPort;

    public string Namespace => _fixture.Namespace;

    public bool IsStarted => Volatile.Read(ref _started) == 1;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Interlocked.CompareExchange(ref _started, 1, 0) != 0)
            return;

        await _fixture.StartAsync().ConfigureAwait(false);
    }

    public async Task StopAsync()
    {
        if (Interlocked.Exchange(ref _started, 0) == 0)
            return;

        await _fixture.StopAsync().ConfigureAwait(false);
    }

    /// <summary>Builds client options pointed at this broker.</summary>
    public ServiceBusClientOptions CreateClientOptions() => new()
    {
        Provider = ServiceBusProvider.Almost,
        ConnectionString = ConnectionString,
        PublicPort = PublicPort,
    };

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        await _fixture.DisposeAsync().ConfigureAwait(false);
    }
}
