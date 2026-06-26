namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct EmployeeId(Guid Value)
{
    internal static EmployeeId New() => new(Guid.CreateVersion7());
}
