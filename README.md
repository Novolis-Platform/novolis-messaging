<!-- novolis-marketing:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-brand-transparent.svg" width="360" alt="Novolis"/>
  </a>
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/banners/novolis-messaging.svg" width="100%" alt="novolis-messaging"/>
</p>

<p align="center">
  <strong>Channels and messaging cores</strong><br/>
  Messaging abstractions and channel-based transports for realtime systems.
</p>

<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-messaging/actions"><img src="https://img.shields.io/github/actions/workflow/status/Novolis-Platform/novolis-messaging/merge.yml?branch=main&label=merge&logo=github" alt="merge"/></a>
  <a href="https://github.com/orgs/Novolis-Platform/packages?repo_name=novolis-messaging"><img src="https://img.shields.io/badge/packages-GitHub%20Packages-0a7ea3?logo=nuget" alt="packages"/></a>
  <a href="https://github.com/Novolis-Platform"><img src="https://img.shields.io/badge/org-Novolis--Platform-111827" alt="org"/></a>
</p>

<p align="center">
  <a href="https://nuget.pkg.github.com/Novolis-Platform/index.json"><code>https://nuget.pkg.github.com/Novolis-Platform/index.json</code></a>
  ·
  <a href="https://github.com/Novolis-Platform/.github/blob/main/profile/README.md">Org landing</a>
  ·
  <a href="https://github.com/Novolis-Platform/novolis-governance">Governance</a>
</p>

---
<!-- novolis-marketing:end -->
<!-- novolis-package-index:start -->
> **GitHub Packages shows this repository README on every package page** (upstream limitation).
> Open the **package README** for install and quick start — embedded in each .nupkg and linked below.

## Published packages

| Package | Install | Package README |
|---------|---------|----------------|
| `Novolis.Messaging` | `dotnet add package Novolis.Messaging` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging/README.md) |
| `Novolis.Messaging.Abstractions` | `dotnet add package Novolis.Messaging.Abstractions` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.Abstractions/README.md) |
| `Novolis.Messaging.Channels` | `dotnet add package Novolis.Messaging.Channels` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.Channels/README.md) |
| `Novolis.Messaging.Coordination.Abstractions` | `dotnet add package Novolis.Messaging.Coordination.Abstractions` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.Coordination.Abstractions/README.md) |
| `Novolis.Messaging.Coordination.InMemory` | `dotnet add package Novolis.Messaging.Coordination.InMemory` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.Coordination.InMemory/README.md) |
| `Novolis.Messaging.Coordination.Redis` | `dotnet add package Novolis.Messaging.Coordination.Redis` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.Coordination.Redis/README.md) |
| `Novolis.Messaging.ServiceBus.Primitives` | `dotnet add package Novolis.Messaging.ServiceBus.Primitives` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.ServiceBus.Primitives/README.md) |
| `Novolis.Messaging.ServiceBus.Abstractions` | `dotnet add package Novolis.Messaging.ServiceBus.Abstractions` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.ServiceBus.Abstractions/README.md) |
| `Novolis.Messaging.ServiceBus.Client` | `dotnet add package Novolis.Messaging.ServiceBus.Client` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.ServiceBus.Client/README.md) |
| `Novolis.Messaging.ServiceBus.Broker.Almost` | `dotnet add package Novolis.Messaging.ServiceBus.Broker.Almost` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.ServiceBus.Broker.Almost/README.md) |

For NuGet.org and Visual Studio, the **embedded** README.md inside each package is authoritative.

<!-- novolis-package-index:end -->
# Messaging

In-process messaging for .NET: **channels** (`System.Threading.Channels` + DI), **PulseFlow** (`Novolis.Messaging`), typed pub/sub contracts, multi-instance **coordination** ports (InMemory / Redis), and **Service Bus** (Azure SDK client + Almost broker).

## Packages

| Package | Description |
|---------|-------------|
| `Novolis.Messaging.Channels` | Register bounded/unbounded channels in `IServiceCollection` |
| `Novolis.Messaging` | Pulse/conduit/flow pipeline (migrated from Frank.PulseFlow) |
| `Novolis.Messaging.Abstractions` | `Message<T>`, publisher/subscriber contracts |
| `Novolis.Messaging.Coordination.Abstractions` | Presence, tick leadership, JWT denylist, rate limit ports |
| `Novolis.Messaging.Coordination.InMemory` | Single-process coordination for dev/tests |
| `Novolis.Messaging.Coordination.Redis` | Redis/Garnet-backed coordination |
| `Novolis.Messaging.ServiceBus.Primitives` | Service Bus `IMessage&lt;T&gt;` / `Message&lt;T&gt;` (+ `.Advanced`) |
| `Novolis.Messaging.ServiceBus.Abstractions` | Service Bus client / admin ports |
| `Novolis.Messaging.ServiceBus.Client` | Azure SDK adapter (cloud + Almost) |
| `Novolis.Messaging.ServiceBus.Broker.Almost` | AlmostServiceBus in-process broker |

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

