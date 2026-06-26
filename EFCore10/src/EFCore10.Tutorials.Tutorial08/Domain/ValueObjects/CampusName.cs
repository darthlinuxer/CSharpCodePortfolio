namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record CampusName
{
    private CampusName(string value) => Value = value;

    public const int MaxLength = 120;

    public string Value { get; }

    internal static CampusName Create(string? value) =>
        new(DomainText.Required(value, "Campus name", minLength: 3, MaxLength));

    internal static CampusName FromStorage(string value) => Create(value);
}
