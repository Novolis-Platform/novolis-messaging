namespace Novolis.Messaging;

/// <summary>
/// Thrown when a pulse is routed to a flow that cannot handle its type.
/// </summary>
/// <param name="s">Human-readable explanation of the mismatch.</param>
public class IncompatibleFlowException(string s) : Exception(s);
