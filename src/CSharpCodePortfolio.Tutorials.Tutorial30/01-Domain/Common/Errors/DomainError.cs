namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;

/// <summary>
/// Stable serializable code for expected domain failures.
/// </summary>
public readonly record struct DomainErrorCode(string Value)
{
    /// <summary>
    /// Returns the wire-safe code value.
    /// </summary>
    public override string ToString() => Value;
}

/// <summary>
/// Domain taxonomy for expected failures before an adapter maps them to transport semantics.
/// </summary>
public readonly record struct DomainErrorCategory(string Value)
{
    /// <summary>
    /// Invalid input or invariant violation.
    /// </summary>
    public static readonly DomainErrorCategory Validation = new("validation");

    /// <summary>
    /// Uniqueness or state conflict.
    /// </summary>
    public static readonly DomainErrorCategory Conflict = new("conflict");
}

/// <summary>
/// Base type for expected domain failures with a stable code and a human message.
/// </summary>
public abstract record DomainError(
    DomainErrorCode Code,
    string Message)
{
    /// <summary>
    /// Gets the domain-level error category.
    /// </summary>
    public abstract DomainErrorCategory Category { get; }
}
