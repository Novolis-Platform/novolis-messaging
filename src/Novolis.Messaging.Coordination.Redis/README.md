<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-messaging">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Messaging.Coordination.Redis

StackExchange.Redis-backed coordination (Garnet/Redis-compatible) with configurable key prefix. Implements session presence, tick leadership, JWT denylist, and rate limiting.

## Install

```bash
dotnet add package Novolis.Messaging.Coordination.Redis
```

**Prerequisite:** `IConnectionMultiplexer` must already be registered (e.g. Aspire `AddRedisClient("garnet")`).

## Quick start

```csharp
using Microsoft.Extensions.DependencyInjection;
using Novolis.Messaging.Coordination.Abstractions;
using Novolis.Messaging.Coordination.Redis;
using StackExchange.Redis;

services.Configure<CoordinationHostingOptions>(
    configuration.GetSection(CoordinationHostingOptions.SectionName));

services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(configuration.GetConnectionString("garnet")!));

services.AddRedisCoordinationImplementations();
// alias: AddGarnetCoordinationImplementations()
```

Optional health check (register separately):

```csharp
services.AddHealthChecks().AddCheck<RedisCoordinationHealthCheck>("redis-coordination");
```

Key layout (prefix from `CoordinationHostingOptions.KeyPrefix`, default `scr:`):

- Presence: `{prefix}prt:{sessionTicks}:{connHash}`
- Tick leader: `{prefix}sim:tick-leader`
- JWT deny: `{prefix}auth:deny:jti:{hash}`
- Rate limit: `{prefix}rl:{hash}`

## API

| Type | Role |
|------|------|
| `RedisCoordinationServiceCollectionExtensions.AddRedisCoordinationImplementations` | Registers all four coordination ports |
| `RedisSessionRealtimePresence` | TTL-scoped connection keys |
| `RedisSimulationTickLeadership` | Distributed lease renewal |
| `RedisTokenDenylist` | JTI deny with TTL |
| `RedisRateLimitCounter` | Distributed fixed-window counter |
| `RedisCoordinationHealthCheck` / `GarnetCoordinationHealthCheck` | Redis ping health check |

## Related

| Package | Role |
|---------|------|
| `Novolis.Messaging.Coordination.Abstractions` | Port definitions and options |
| `Novolis.Messaging.Coordination.InMemory` | Single-process fallback for dev/tests |

