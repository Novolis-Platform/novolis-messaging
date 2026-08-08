<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-messaging">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Messaging.ServiceBus.Primitives

Typed Service Bus envelope: `IMessage<T>` / `Message<T>`, with broker metadata behind `Advanced`.

## Install

```bash
dotnet add package Novolis.Messaging.ServiceBus.Primitives
```

## Quick start

```csharp
using Novolis.Messaging.ServiceBus;

var message = new Message<OrderPlaced>(new OrderPlaced(42));
// everyday: message.Payload, message.Id, message.CorrelationId, message.Subject

// dig when needed:
var lockToken = message.Advanced.LockToken;
var props = message.Advanced.ApplicationProperties;
```

## Related

| Package | Role |
|---------|------|
| `Novolis.Messaging.ServiceBus.Abstractions` | Client / admin ports |
| `Novolis.Messaging.ServiceBus.Client` | Azure SDK adapter |
