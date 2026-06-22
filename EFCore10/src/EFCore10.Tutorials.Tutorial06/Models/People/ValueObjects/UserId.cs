namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct UserId
{
    private UserId(Guid value) => Value = value;

    public Guid Value { get; }

    public static UserId NewId() => From(Guid.CreateVersion7());

    public static UserId From(Guid value) =>
        value == Guid.Empty ? throw new DomainException("User ID is required.") : new UserId(value);

    public override string ToString() => Value.ToString();
}
