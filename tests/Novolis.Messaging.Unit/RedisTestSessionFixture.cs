using StackExchange.Redis;
using Testcontainers.Redis;
using TUnit.Core;

namespace Novolis.Messaging.Unit;

public static class RedisTestSessionFixture
{
    private static RedisContainer? _container;
    private static IConnectionMultiplexer? _multiplexer;
    private static string? _connectionString;

    public static IConnectionMultiplexer Multiplexer =>
        _multiplexer ?? throw new InvalidOperationException("Redis test session fixture is not initialized.");

    public static string ConnectionString =>
        _connectionString ?? throw new InvalidOperationException("Redis test session fixture is not initialized.");

    [Before(TestSession)]
    public static async Task StartAsync()
    {
        _container = new RedisBuilder().Build();
        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();
        _multiplexer = await ConnectionMultiplexer.ConnectAsync(_connectionString);
    }

    [After(TestSession)]
    public static async Task StopAsync()
    {
        if (_multiplexer is not null)
        {
            await _multiplexer.DisposeAsync();
            _multiplexer = null;
        }

        if (_container is not null)
        {
            await _container.DisposeAsync();
            _container = null;
        }
    }
}
