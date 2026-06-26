namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record DepartmentName
{
    private DepartmentName(string value) => Value = value;

    public const int MaxLength = 120;

    public string Value { get; }

    internal static DepartmentName Create(string? value) =>
        new(DomainText.Required(value, "Department name", minLength: 3, MaxLength));

    internal static DepartmentName FromStorage(string value) => Create(value);
}
