# Design

## Packages

| Package | Responsibility |
|---------|----------------|
| `Novolis.Messaging.Channels` | Singleton `Channel<T>`, reader, and writer registration |
| `Novolis.Messaging` | Pulse types, flows, conduits, and handler dispatch |

## Channels

`ChannelSettings` controls single-reader/writer defaults and bounded queue behavior. Factories are registered once per service collection.

## Messaging

Pulses carry a `Guid` and timestamp. Flows match pulse types to `IPulseHandler` implementations. Misrouted pulses raise `IncompatibleFlowException`.

## Consumers

**Frank.WireFish** uses channels for `DevicePacket` fan-out; application services can combine both packages for domain events.
