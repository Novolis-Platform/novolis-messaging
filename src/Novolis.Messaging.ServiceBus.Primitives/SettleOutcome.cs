namespace Novolis.Messaging.ServiceBus;

/// <summary>Result of settling a peek-locked message.</summary>
public enum SettleOutcome
{
    Completed = 0,
    Abandoned = 1,
    DeadLettered = 2,
}
