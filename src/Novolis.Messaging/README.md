# Novolis.Messaging

Pulse-based in-process messaging with composable flows and handlers.

## Install

```bash
dotnet add package Novolis.Messaging
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
services.AddPulseMessaging(builder =>
{
    builder.AddFlow<MyPulse, MyHandler>();
});
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Messaging.Channels` | Low-level channel registration (dependency) |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-messaging/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-messaging/blob/main/docs/design.md)

## Support

Pre-release API; flows require matching pulse types or `IncompatibleFlowException` is thrown.
