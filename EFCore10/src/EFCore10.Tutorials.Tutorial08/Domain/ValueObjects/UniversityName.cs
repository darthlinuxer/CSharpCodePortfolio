namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct UniversityName(string Value)
{
    public const int MaxLength = 160;

    internal static UniversityName Create(string? value) =>
        new(DomainText.Required(value, "University name", minLength: 3, MaxLength));
}
