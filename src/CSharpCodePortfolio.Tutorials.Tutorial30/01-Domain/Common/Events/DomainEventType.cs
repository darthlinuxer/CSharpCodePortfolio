namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Events;

/// <summary>
/// Value object that names a domain event without exposing raw strings as the public contract.
/// </summary>
public readonly record struct DomainEventType(string Value)
{
    /// <summary>
    /// Returns the stable event name for logs, HTTP evidence, and tests.
    /// </summary>
    public override string ToString() => Value;
}
