namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct PersonId
{
    private PersonId(Guid value) => Value = value;

    public Guid Value { get; }

    public static PersonId NewId() => From(Guid.CreateVersion7());

    public static PersonId From(Guid value) =>
        value == Guid.Empty ? throw new DomainException("Person ID is required.") : new PersonId(value);

    public override string ToString() => Value.ToString();
}
