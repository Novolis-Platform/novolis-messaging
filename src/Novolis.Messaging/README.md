<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-messaging">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Messaging

Pulse-based in-process messaging (PulseFlow): pulses are written to a `Channel<IPulse>`, consumed by `PulseNexus` (`BackgroundService`), and dispatched to registered `IFlow` instances.

## Install

```bash
dotnet add package Novolis.Messaging
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

Depends on `Novolis.Messaging.Channels`.

## Quick start — handler flow

```csharp
using Novolis.Messaging;

public sealed record TimerPulse(string Label) : BasePulse;

public sealed class TimerHandler : IPulseHandler<TimerPulse>
{
    public Task HandleAsync(TimerPulse pulse, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

services.AddPulseFlow<TimerPulse, TimerHandler>();
```

## Quick start — custom flow

```csharp
public sealed class MyFlow : IFlow
{
    public bool CanHandle(Type pulseType) => pulseType == typeof(MyPulse);
    public Task HandleAsync(IPulse pulse, CancellationToken cancellationToken) => Task.CompletedTask;
}

services.AddPulseFlow(builder => builder.AddFlow<MyFlow>());
// shorthand: services.AddPulseFlow<MyFlow>();
```

Send pulses from any service:

```csharp
await conduit.SendAsync(new TimerPulse("tick"), cancellationToken);
```

Multiple handlers for the same pulse type are supported (`AddPulseFlow<TPulse, THandler2>()`). Unmatched pulses and flow faults can be observed via `ConfigurePulseFlowDiagnostics`.

## API

| Type | Role |
|------|------|
| `IPulse` / `BasePulse` | Pulse identity (`Id`, `Created`) |
| `IPulseHandler<T>` | Typed handler for a pulse |
| `IFlow` | Multi-pulse dispatcher (`CanHandle`, `HandleAsync`) |
| `IFlowBuilder` | `AddFlow<T>()` during registration |
| `IConduit` | `SendAsync(IPulse, CancellationToken)` |
| `ServiceCollectionExtensions.AddPulseFlow` | Register flows + hosted `PulseNexus` |
| `ConfigurePulseFlowDiagnostics` | Unmatched/fault callbacks |
| `IncompatibleFlowException` | Thrown on pulse type mismatch in a flow |

## Related

| Package | Role |
|---------|------|
| `Novolis.Messaging.Channels` | `Channel<T>` DI registration |
| `Novolis.Messaging.Abstractions` | Typed pub/sub envelope (separate model) |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-messaging/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-messaging/blob/main/docs/design.md)

## Support

Pre-release API; flows require matching pulse types or `IncompatibleFlowException` is thrown.

