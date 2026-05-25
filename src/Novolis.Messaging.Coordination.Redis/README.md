# Novolis.Messaging.Coordination.Redis

Redis/Garnet-backed coordination with configurable key prefix.

## Install

```bash
dotnet add package Novolis.Messaging.Coordination.Redis
```

## Quick start

```csharp
// IConnectionMultiplexer must already be registered (e.g. Aspire AddRedisClient).
services.AddRedisCoordinationImplementations();
```
