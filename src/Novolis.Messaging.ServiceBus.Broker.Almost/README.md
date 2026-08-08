<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-messaging">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Messaging.ServiceBus.Broker.Almost

Hosts [AlmostServiceBus](https://github.com/gkinsman/AlmostServiceBus) in-process for local development and tests. Pair with `Novolis.Messaging.ServiceBus.Client`.

## Install

```bash
dotnet add package Novolis.Messaging.ServiceBus.Broker.Almost
```

## Quick start

```csharp
await using var broker = new AlmostServiceBusBroker();
await broker.StartAsync();

services.AddServiceBusClient(_ => { }); // options filled when using AddAlmostServiceBusBroker
// or:
var client = new AzureServiceBusClient(broker.CreateClientOptions());
```

DI:

```csharp
services.AddAlmostServiceBusBroker();
services.AddServiceBusClient();
```
