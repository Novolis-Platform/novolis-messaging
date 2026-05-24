# Novolis.Messaging.Channels

Registers `System.Threading.Channels` with dependency injection for producer/consumer patterns.

## Install

```bash
dotnet add package Novolis.Messaging.Channels
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
services.AddChannel<MyMessage>();
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Messaging` | Pulse/flow messaging on top of channels |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-messaging/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-messaging/blob/main/docs/design.md)

## Support

Pre-release; channel lifetime follows ASP.NET Core hosted service defaults.
