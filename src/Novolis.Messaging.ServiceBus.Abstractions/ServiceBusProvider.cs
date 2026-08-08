namespace Novolis.Messaging.ServiceBus.Abstractions;

/// <summary>Which broker the client targets.</summary>
public enum ServiceBusProvider
{
    /// <summary>Azure Service Bus cloud (or any connection string without the Almost host).</summary>
    Azure = 0,

    /// <summary>AlmostServiceBus (or MS development emulator) — plaintext + UseDevelopmentEmulator.</summary>
    Almost = 1,
}
