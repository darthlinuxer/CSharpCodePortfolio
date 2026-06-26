namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct StaffRole(string Value)
{
    public const int MaxLength = 80;

    internal static StaffRole Create(string? value) =>
        new(DomainText.Required(value, "Staff role", minLength: 3, MaxLength));
}
