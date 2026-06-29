namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

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
/// Base type for expected domain failures with a stable code and a human message.
/// </summary>
public abstract record DomainError(
    DomainErrorCode Code,
    string Message);
