namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct PersonId(Guid Value)
{
    public static PersonId NewId() => new(Guid.NewGuid());
}
