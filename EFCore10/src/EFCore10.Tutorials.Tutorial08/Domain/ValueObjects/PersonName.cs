namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record PersonName
{
    private PersonName(string value) => Value = value;

    public const int MaxLength = 160;

    public string Value { get; }

    internal static PersonName Create(string? value) =>
        new(DomainText.Required(value, "Person name", minLength: 3, MaxLength));

    internal static PersonName FromStorage(string value) => Create(value);
}
