namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct BlogOwnerId
{
    private BlogOwnerId(Guid value) => Value = value;

    public Guid Value { get; }

    public static BlogOwnerId NewId() => From(Guid.CreateVersion7());

    public static BlogOwnerId From(Guid value) =>
        value == Guid.Empty ? throw new DomainException("Blog owner ID is required.") : new BlogOwnerId(value);

    public override string ToString() => Value.ToString();
}
