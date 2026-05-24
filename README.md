<!-- novolis-package-index:start -->
> **GitHub Packages shows this repository README on every package page** (upstream limitation).
> Open the **package README** for install and quick start — embedded in each .nupkg and linked below.

## Published packages

| Package | Install | Package README |
|---------|---------|----------------|
| `Novolis.Messaging` | `dotnet add package Novolis.Messaging` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging/README.md) |
| `Novolis.Messaging.Channels` | `dotnet add package Novolis.Messaging.Channels` | [README](https://github.com/Novolis-Platform/novolis-messaging/blob/main/src/Novolis.Messaging.Channels/README.md) |

For NuGet.org and Visual Studio, the **embedded** README.md inside each package is authoritative.

<!-- novolis-package-index:end -->

# Messaging

In-process messaging for .NET: **channels** (`System.Threading.Channels` + DI) and **PulseFlow** (`Novolis.Messaging`).

## Packages

| Package | Description |
|---------|-------------|
| `Novolis.Messaging.Channels` | Register bounded/unbounded channels in `IServiceCollection` |
| `Novolis.Messaging` | Pulse/conduit/flow pipeline (migrated from Frank.PulseFlow) |

## Install

```bash
dotnet add package Novolis.Messaging.Channels --version 0.1.0-preview.1
dotnet add package Novolis.Messaging --version 0.1.0-preview.1
```

Preview builds publish from this repo after NuGet trusted publishing is configured ([release](docs/release.md)).

## Quick start

```csharp
services.AddChannel<MyEvent>();
services.AddPulseFlow<MyFlow>();
```

## Documentation

- [Getting started](docs/getting-started.md)
- [Design](docs/design.md)
- [Release](docs/release.md)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

