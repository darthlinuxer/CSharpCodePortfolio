namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;

public readonly record struct DomainErrorCode(string Value)
{
    public override string ToString() => Value;
}

public readonly record struct DomainErrorCategory(string Value)
{
    public static DomainErrorCategory Validation { get; } = new("validation");
    public static DomainErrorCategory Conflict { get; } = new("conflict");

    public override string ToString() => Value;
}

public abstract record DomainError(
    DomainErrorCode Code,
    string Message)
{
    public abstract DomainErrorCategory Category { get; }
}
