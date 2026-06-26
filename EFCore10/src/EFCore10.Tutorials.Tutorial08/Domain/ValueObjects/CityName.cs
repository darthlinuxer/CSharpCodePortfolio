namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record CityName
{
    private CityName(string value) => Value = value;

    public const int MaxLength = 100;

    public string Value { get; }

    internal static CityName Create(string? value) =>
        new(DomainText.Required(value, "Campus city", minLength: 3, MaxLength));

    internal static CityName FromStorage(string value) => Create(value);
}
