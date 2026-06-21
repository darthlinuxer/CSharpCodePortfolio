namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct BlogId(Guid Value)
{
    public static BlogId NewId() => new(Guid.NewGuid());
}
