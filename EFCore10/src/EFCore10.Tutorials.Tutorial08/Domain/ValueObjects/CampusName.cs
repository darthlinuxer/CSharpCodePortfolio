namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct CampusName(string Value)
{
    public const int MaxLength = 120;

    internal static CampusName Create(string? value) =>
        new(DomainText.Required(value, "Campus name", minLength: 3, MaxLength));
}
