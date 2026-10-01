<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-messaging/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-messaging/) · [Source](https://github.com/Novolis-Platform/novolis-messaging)
<!-- novolis-pkg-brand:end -->

# Novolis.Messaging.Coordination.InMemory

In-process coordination for single-process hosts and unit tests. Does not share state across pods or processes.

## Install

```bash
dotnet add package Novolis.Messaging.Coordination.InMemory
```

Depends on `Novolis.Messaging.Coordination.Abstractions`.

## Quick start

```csharp
using Microsoft.Extensions.DependencyInjection;
using Novolis.Messaging.Coordination.InMemory;

services.AddInMemoryCoordination();
```

```csharp
var counter = sp.GetRequiredService<IRateLimitCounter>();
var count = await counter.IncrementAsync("login:user123", TimeSpan.FromMinutes(1));
```

## API

| Type | Role |
|------|------|
| `InMemoryCoordinationServiceCollectionExtensions.AddInMemoryCoordination` | Registers all four coordination ports |
| `InMemorySessionRealtimePresence` | In-process connection tracking |
| `InMemorySimulationTickLeadership` | Always succeeds (`TryRenewOrAcquireAsync` → `true`) |
| `InMemoryTokenDenylist` | No-op denylist (`IsDeniedAsync` always `false`) |
| `InMemoryRateLimitCounter` | Per-process fixed-window counter |

## Behavior notes

- Presence and rate limits are **not** shared across instances.
- Token denylist is a stub — use Redis coordination for real JWT revocation.

## Related

| Package | Role |
|---------|------|
| `Novolis.Messaging.Coordination.Abstractions` | Port definitions and `CoordinationHostingOptions` |
| `Novolis.Messaging.Coordination.Redis` | Production multi-instance coordination |

