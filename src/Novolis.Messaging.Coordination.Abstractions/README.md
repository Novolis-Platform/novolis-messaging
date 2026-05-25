# Novolis.Messaging.Coordination.Abstractions

Distributed host coordination ports for multi-instance ASP.NET hosts.

## Install

```bash
dotnet add package Novolis.Messaging.Coordination.Abstractions
```

## Quick start

```csharp
// Register an implementation package (InMemory or Redis), then inject:
// ISessionRealtimePresence, ISimulationTickLeadership, ITokenDenylist, IRateLimitCounter
```
