using Microsoft.Extensions.DependencyInjection;
using Novolis.Messaging.Coordination.Abstractions;

namespace Novolis.Messaging.Coordination.InMemory;

public static class InMemoryCoordinationServiceCollectionExtensions
{
    /// <summary>Registers in-process coordination (default for desktop hosts and tests).</summary>
    public static IServiceCollection AddInMemoryCoordination(this IServiceCollection services)
    {
        services.AddSingleton<InMemorySessionRealtimePresence>();
        services.AddSingleton<ISessionRealtimePresence>(sp => sp.GetRequiredService<InMemorySessionRealtimePresence>());
        services.AddSingleton<ISimulationTickLeadership, InMemorySimulationTickLeadership>();
        services.AddSingleton<ITokenDenylist, InMemoryTokenDenylist>();
        services.AddSingleton<IRateLimitCounter, InMemoryRateLimitCounter>();
        return services;
    }
}
