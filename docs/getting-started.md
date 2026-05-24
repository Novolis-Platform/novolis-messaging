# Getting started

Novolis messaging ships two packages: **Novolis.Messaging.Channels** for DI-friendly `Channel<T>` registration, and **Novolis.Messaging** for pulse/flow routing.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Build

```bash
dotnet build Novolis.Messaging.slnx
```

## Channels

```csharp
services.AddChannel<DevicePacket>(ChannelType.Bounded, new ChannelSettings { BoundedCapacity = 256 });
```

## Pulses and flows

Register flows that handle specific `IPulse` types and route them through `IConduit` implementations. See package READMEs for extension method names on `IServiceCollection`.

## See also

- [Design](design.md)
- [Release](release.md)
