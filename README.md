<!-- novolis-package-index:start -->
> **GitHub Packages shows this repository README on every package page** (upstream limitation).
> Open the **package README** for install and quick start — embedded in each .nupkg and linked below.

## Published packages

| Package | Install | Package README |
|---------|---------|----------------|
| `Novolis.Messaging.Abstractions` | `dotnet add package Novolis.Messaging.Abstractions` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.Abstractions/README.md) |
| `Novolis.Messaging.Channels` | `dotnet add package Novolis.Messaging.Channels` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.Channels/README.md) |
| `Novolis.Messaging` | `dotnet add package Novolis.Messaging` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging/README.md) |
| `Novolis.Messaging.Coordination.Abstractions` | `dotnet add package Novolis.Messaging.Coordination.Abstractions` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.Coordination.Abstractions/README.md) |
| `Novolis.Messaging.Coordination.InMemory` | `dotnet add package Novolis.Messaging.Coordination.InMemory` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.Coordination.InMemory/README.md) |
| `Novolis.Messaging.Coordination.Redis` | `dotnet add package Novolis.Messaging.Coordination.Redis` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.Coordination.Redis/README.md) |

For NuGet.org and Visual Studio, the **embedded** README.md inside each package is authoritative.

<!-- novolis-package-index:end -->

# Messaging

In-process messaging for .NET: **channels** (`System.Threading.Channels` + DI), **PulseFlow** (`Novolis.Messaging`), typed pub/sub contracts, and multi-instance **coordination** ports (InMemory / Redis).

## Packages

| Package | Description |
|---------|-------------|
| `Novolis.Messaging.Channels` | Register bounded/unbounded channels in `IServiceCollection` |
| `Novolis.Messaging` | Pulse/conduit/flow pipeline (migrated from Frank.PulseFlow) |
| `Novolis.Messaging.Abstractions` | `Message<T>`, publisher/subscriber contracts |
| `Novolis.Messaging.Coordination.Abstractions` | Presence, tick leadership, JWT denylist, rate limit ports |
| `Novolis.Messaging.Coordination.InMemory` | Single-process coordination for dev/tests |
| `Novolis.Messaging.Coordination.Redis` | Redis/Garnet-backed coordination |

## Quick start

```csharp
services.AddChannel<MyEvent>();
services.AddPulseFlow<TimerPulse, TimerHandler>();
services.AddInMemoryCoordination();
```

## Documentation

- [Getting started](docs/getting-started.md)
- [Design](docs/design.md)
- [Release](docs/release.md)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).
