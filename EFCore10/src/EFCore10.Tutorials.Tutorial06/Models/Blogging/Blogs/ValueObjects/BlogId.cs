namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct BlogId
{
    private BlogId(Guid value) => Value = value;

    public Guid Value { get; }

    public static BlogId NewId() => From(Guid.CreateVersion7());

    public static BlogId From(Guid value) =>
        value == Guid.Empty ? throw new DomainException("Blog ID is required.") : new BlogId(value);

    public override string ToString() => Value.ToString();
}
