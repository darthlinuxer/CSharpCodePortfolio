namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct StudentId(Guid Value)
{
    internal static StudentId New() => new(Guid.CreateVersion7());
}
