# Design

## Packages

| Package | Responsibility |
|---------|----------------|
| `Novolis.Messaging.Channels` | Singleton `Channel<T>`, reader, and writer registration |
| `Novolis.Messaging` | Pulse types, flows, conduits, and handler dispatch |
| `Novolis.Messaging.ServiceBus.*` | Service Bus facet: primitives, ports, Azure client, Almost broker |

## Channels

`ChannelSettings` controls single-reader/writer defaults and bounded queue behavior. Factories are registered once per service collection.

## Messaging

Pulses carry a `Guid` and timestamp. Flows match pulse types to `IPulseHandler` implementations. Misrouted pulses raise `IncompatibleFlowException`.

## Service Bus

`Novolis.Messaging.ServiceBus.Message<T>` is separate from in-process `Novolis.Messaging.Message<T>`. Everyday fields are payload and ids; broker metadata lives on `message.Advanced`. One Client (`Azure.Messaging.ServiceBus`) targets cloud Azure or AlmostServiceBus.

## Consumers

**Frank.WireFish** uses channels for `DevicePacket` fan-out; application services can combine both packages for domain events.
