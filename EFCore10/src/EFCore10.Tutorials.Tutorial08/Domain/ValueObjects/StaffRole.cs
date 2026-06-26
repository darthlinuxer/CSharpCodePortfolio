namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record StaffRole
{
    private StaffRole(string value) => Value = value;

    public const int MaxLength = 80;

    public string Value { get; }

    internal static StaffRole Create(string? value) =>
        new(DomainText.Required(value, "Staff role", minLength: 3, MaxLength));

    internal static StaffRole FromStorage(string value) => Create(value);
}
