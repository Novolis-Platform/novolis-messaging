# Novolis.Messaging.Abstractions

Message envelope and publisher/subscriber contracts (Frank.Messaging migration).

## Install

```bash
dotnet add package Novolis.Messaging.Abstractions
```

## Quick start

```csharp
public sealed record PlayerJoined(Guid PlayerId);
// Implement IMessagePublisher<PlayerJoined> / IMessageSubscriber<PlayerJoined> in your host.
```
