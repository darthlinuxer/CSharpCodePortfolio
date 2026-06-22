namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct AuthorId
{
    private AuthorId(Guid value) => Value = value;

    public Guid Value { get; }

    public static AuthorId NewId() => From(Guid.CreateVersion7());

    public static AuthorId From(Guid value) =>
        value == Guid.Empty ? throw new DomainException("Author ID is required.") : new AuthorId(value);

    public override string ToString() => Value.ToString();
}
