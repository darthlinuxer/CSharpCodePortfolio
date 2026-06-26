namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct CourseId(Guid Value)
{
    internal static CourseId New() => new(Guid.CreateVersion7());
}
