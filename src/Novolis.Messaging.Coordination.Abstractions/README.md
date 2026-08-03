<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-messaging">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Messaging.Coordination.Abstractions

Distributed host coordination ports for multi-instance ASP.NET hosts: session presence, simulation tick leadership, JWT denylist, and rate limiting.

## Install

```bash
dotnet add package Novolis.Messaging.Coordination.Abstractions
```

Register an implementation package (`Novolis.Messaging.Coordination.InMemory` or `.Redis`), then bind options from configuration.

## Quick start

```csharp
using Microsoft.Extensions.DependencyInjection;
using Novolis.Messaging.Coordination.Abstractions;

services.Configure<CoordinationHostingOptions>(
    configuration.GetSection(CoordinationHostingOptions.SectionName));

services.AddInMemoryCoordination(); // or AddRedisCoordinationImplementations()

// Inject in handlers / background services:
ISessionRealtimePresence presence;
ISimulationTickLeadership tickLeadership;
ITokenDenylist denylist;
IRateLimitCounter rateLimit;
```

## API

| Type | Role |
|------|------|
| `CoordinationHostingOptions` | `Mode`, `RedisConnectionName`, `RequireDistributedTickLeadership`, `TickLeadershipLeaseSeconds`, `InstanceId`, `PresenceKeyExpirySeconds`, `KeyPrefix` (default `"scr:"`) |
| `ISessionRealtimePresence` | Track/untrack connections per session; `GetSubscriberCount` |
| `ISimulationTickLeadership` | `TryRenewOrAcquireAsync` — distributed tick leader lease |
| `ITokenDenylist` | `IsDeniedAsync`, `DenyAsync(jti, ttl)` |
| `IRateLimitCounter` | `IncrementAsync(bucketKey, windowTtl)` → count in window |

## Related

| Package | Role |
|---------|------|
| `Novolis.Messaging.Coordination.InMemory` | Single-process stubs for dev/tests |
| `Novolis.Messaging.Coordination.Redis` | Redis/Garnet-backed implementations |

