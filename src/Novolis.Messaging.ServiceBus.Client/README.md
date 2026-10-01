<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-messaging/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-messaging/) · [Source](https://github.com/Novolis-Platform/novolis-messaging)
<!-- novolis-pkg-brand:end -->

# Novolis.Messaging.ServiceBus.Client

`Azure.Messaging.ServiceBus` adapter for Novolis Service Bus ports. One client for cloud Azure and AlmostServiceBus (`UseDevelopmentEmulator` + optional `PublicPort`).

## Install

```bash
dotnet add package Novolis.Messaging.ServiceBus.Client
```

## Quick start

```csharp
services.AddServiceBusClient(o =>
{
    o.Provider = ServiceBusProvider.Azure;
    o.ConnectionString = "<azure-connection-string>";
});

// or Almost (after AddAlmostServiceBusBroker):
services.AddServiceBusClient(o =>
{
    o.Provider = ServiceBusProvider.Almost;
    o.ConnectionString = broker.ConnectionString;
    o.PublicPort = broker.PublicPort;
});
```

## Related

| Package | Role |
|---------|------|
| `Novolis.Messaging.ServiceBus.Broker.Almost` | Local AlmostServiceBus host |
| `Novolis.Testing.ServiceBus` | TUnit fixture |
