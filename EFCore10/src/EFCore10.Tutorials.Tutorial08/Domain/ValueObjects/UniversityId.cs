namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct UniversityId(Guid Value)
{
    internal static UniversityId New() => new(Guid.CreateVersion7());
}
