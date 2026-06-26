namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct CityName(string Value)
{
    public const int MaxLength = 100;

    internal static CityName Create(string? value) =>
        new(DomainText.Required(value, "Campus city", minLength: 3, MaxLength));
}
