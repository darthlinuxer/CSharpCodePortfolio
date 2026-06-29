namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Describes an expected domain failure with a stable code and a human message.
/// </summary>
public sealed record DomainError(
    string Code, 
    string Message);
