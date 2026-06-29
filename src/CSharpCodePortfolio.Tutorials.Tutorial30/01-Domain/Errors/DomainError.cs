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
/// Domain taxonomy for expected failures. The category is a domain concept
/// (validation vs conflict) — NOT an HTTP concept. The presentation layer
/// translates each category through a static mapping table, so it never
/// has to pattern-match concrete domain error types.
/// </summary>
/// <remarks>
/// Marked as <c>readonly record struct</c> for the same reasons the other
/// value objects are: structural equality, zero allocation, and absence of
/// boxing in comparisons and accumulators.
/// </remarks>
public readonly record struct DomainErrorCategory(string Value)
{
    /// <summary>
    /// Failures that arise from invalid input or an aggregate invariant
    /// violation — typically mapped to HTTP 400 at the presentation layer.
    /// </summary>
    public static readonly DomainErrorCategory Validation = new("validation");

    /// <summary>
    /// Failures that arise from uniqueness or state conflicts — typically
    /// mapped to HTTP 409 at the presentation layer.
    /// </summary>
    public static readonly DomainErrorCategory Conflict = new("conflict");
}

/// <summary>
/// Base type for expected domain failures with a stable code, a human message,
/// and a domain category the presentation can dispatch on.
/// </summary>
public abstract record DomainError(
    DomainErrorCode Code,
    string Message)
{
    /// <summary>
    /// Domain-pure category that classifies this failure for downstream
    /// adapters. The presentation layer reads only this property; it never
    /// pattern-matches concrete subclasses.
    /// </summary>
    public abstract DomainErrorCategory Category { get; }
}
