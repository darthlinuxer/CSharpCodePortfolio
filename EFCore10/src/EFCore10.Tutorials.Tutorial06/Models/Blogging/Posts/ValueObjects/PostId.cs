namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct PostId(Guid Value)
{
    public static PostId NewId() => new(Guid.NewGuid());
}
