namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct PersonName(string Value)
{
    public const int MaxLength = 160;

    internal static PersonName Create(string? value) =>
        new(DomainText.Required(value, "Person name", minLength: 3, MaxLength));
}
