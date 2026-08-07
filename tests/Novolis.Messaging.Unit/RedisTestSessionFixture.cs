using StackExchange.Redis;
using Testcontainers.Redis;
using TUnit.Core;

namespace Novolis.Messaging.Unit;

public static class RedisTestSessionFixture
{
    private static RedisContainer? _container;
    private static IConnectionMultiplexer? _multiplexer;
    private static string? _connectionString;
    private static bool _available;

    public static bool IsAvailable => _available;

    public static IConnectionMultiplexer Multiplexer =>
        _multiplexer ?? throw new InvalidOperationException("Redis test session fixture is not initialized.");

    public static string ConnectionString =>
        _connectionString ?? throw new InvalidOperationException("Redis test session fixture is not initialized.");

    [Before(TestSession)]
    public static async Task StartAsync()
    {
        try
        {
            _container = new RedisBuilder().Build();
            await _container.StartAsync();
            _connectionString = _container.GetConnectionString();
            _multiplexer = await ConnectionMultiplexer.ConnectAsync(_connectionString);
            _available = true;
        }
        catch (Exception ex)
        {
            _available = false;
            Console.WriteLine($"Redis Testcontainers unavailable; Redis integration tests will skip. ({ex.GetType().Name}: {ex.Message})");
            if (_container is not null)
            {
                try { await _container.DisposeAsync(); } catch { /* ignore */ }
                _container = null;
            }
        }
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

        _available = false;
        _connectionString = null;
    }
}
