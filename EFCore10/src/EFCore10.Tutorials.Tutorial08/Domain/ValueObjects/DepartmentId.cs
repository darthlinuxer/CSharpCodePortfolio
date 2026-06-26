namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct DepartmentId(Guid Value)
{
    internal static DepartmentId New() => new(Guid.CreateVersion7());
}
