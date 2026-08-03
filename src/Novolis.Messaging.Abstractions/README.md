<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-messaging">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Messaging.Abstractions

Typed message envelope and publisher/subscriber contracts (Frank.Messaging migration). Contracts only — no implementations or DI in this package.

## Install

```bash
dotnet add package Novolis.Messaging.Abstractions
```

## Quick start

```csharp
using Novolis.Messaging;

public sealed record PlayerJoined(Guid PlayerId);

// Implement in your host or wire manually:
public sealed class PlayerJoinedPublisher : IMessagePublisher<PlayerJoined>
{
    public Task PublishAsync(Message<PlayerJoined> message, CancellationToken cancellationToken = default)
        => /* dispatch */;
}

public sealed class PlayerJoinedSubscriber : IMessageSubscriber<PlayerJoined>
{
    public Task SubscribeAsync(
        Func<Message<PlayerJoined>, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        => /* subscribe */;
}
```

`ChannelMessagePublisher<T>` in `Novolis.Messaging.Channels` implements `IMessagePublisher<T>` when you register a channel manually.

## API

| Type | Role |
|------|------|
| `Message<T>` | Envelope: `Id`, `CorrelationId`, `Payload` |
| `IMessagePublisher<T>` | `PublishAsync(Message<T>, CancellationToken)` |
| `IMessageSubscriber<T>` | `SubscribeAsync(handler, CancellationToken)` |
| `IMessagingManager` | `CreateTopicAsync<T>`, `CreateSubscriptionAsync<T>` (port — no shipped implementation) |

## Related

| Package | Role |
|---------|------|
| `Novolis.Messaging.Channels` | `Channel<T>` DI + `ChannelMessagePublisher<T>` |
| `Novolis.Messaging` | PulseFlow in-process messaging (separate model) |

