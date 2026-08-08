<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-messaging">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Messaging.ServiceBus.Abstractions

Ports for Service Bus send/receive/admin and `ServiceBusClientOptions` (`Azure` | `Almost`).

## Install

```bash
dotnet add package Novolis.Messaging.ServiceBus.Abstractions
```

## Quick start

```csharp
using Novolis.Messaging.ServiceBus;

// Register ports via Client (Azure) or Broker.Almost + Client (local):
// services.AddServiceBusClient(...);
// Then resolve IServiceBusClient / IServiceBusAdministration.
```

## Related

| Package | Role |
|---------|------|
| `Novolis.Messaging.ServiceBus.Primitives` | `Message&lt;T&gt;` |
| `Novolis.Messaging.ServiceBus.Client` | Azure SDK implementation |
| `Novolis.Messaging.ServiceBus.Broker.Almost` | AlmostServiceBus host |
