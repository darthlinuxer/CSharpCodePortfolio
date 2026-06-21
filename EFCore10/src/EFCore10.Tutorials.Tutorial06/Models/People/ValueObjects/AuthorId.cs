namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct AuthorId(Guid Value)
{
    public static AuthorId From(PersonId id) => new(id.Value);
}
