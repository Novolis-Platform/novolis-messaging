<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-messaging">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Messaging.Channels

Registers `System.Threading.Channels.Channel<T>` (plus reader/writer) in dependency injection for producer/consumer patterns.

## Install

```bash
dotnet add package Novolis.Messaging.Channels
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Messaging.Channels;

services.AddChannel<MyMessage>();

// Producer: inject ChannelWriter<MyMessage>
// Consumer: inject ChannelReader<MyMessage> (e.g. in a BackgroundService)
```

Bounded channel with custom settings:

```csharp
services.AddChannel<DevicePacket>(
    ChannelType.Bounded,
    new ChannelSettings
    {
        BoundedCapacity = 256,
        BoundedFullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = false,
    });
```

Wire `ChannelMessagePublisher<T>` to `IMessagePublisher<T>` manually if needed:

```csharp
services.AddSingleton<IMessagePublisher<MyMessage>, ChannelMessagePublisher<MyMessage>>();
```

Constraint: `T` must be a reference type (`where T : class`).

## API

| Type | Role |
|------|------|
| `ServiceCollectionExtensions.AddChannel<T>` | Unbounded, bounded, or custom settings overloads |
| `ChannelType` | `Unbounded`, `Bounded` |
| `ChannelSettings` | `SingleReader`, `SingleWriter`, `BoundedCapacity`, `BoundedFullMode` |
| `ChannelMessagePublisher<T>` | `IMessagePublisher<T>` adapter over `ChannelWriter<Message<T>>` |

Each registration adds singleton `Channel<T>`, `ChannelReader<T>`, and `ChannelWriter<T>`.

## Related

| Package | Role |
|---------|------|
| `Novolis.Messaging` | PulseFlow pipeline on top of `Channel<IPulse>` |
| `Novolis.Messaging.Abstractions` | `Message<T>`, `IMessagePublisher<T>` |
| `Novolis.Transports.WireFish` | Publishes captured packets via channels |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-messaging/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-messaging/blob/main/docs/design.md)

## Support

Pre-release; channel lifetime follows ASP.NET Core hosted service defaults.

