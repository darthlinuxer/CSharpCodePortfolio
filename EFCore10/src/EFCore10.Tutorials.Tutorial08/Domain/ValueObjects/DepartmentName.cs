namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct DepartmentName(string Value)
{
    public const int MaxLength = 120;

    internal static DepartmentName Create(string? value) =>
        new(DomainText.Required(value, "Department name", minLength: 3, MaxLength));
}
