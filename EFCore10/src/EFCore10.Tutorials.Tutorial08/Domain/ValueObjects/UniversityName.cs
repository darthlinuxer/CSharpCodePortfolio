namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record UniversityName
{
    private UniversityName(string value) => Value = value;

    public const int MaxLength = 160;

    public string Value { get; }

    internal static UniversityName Create(string? value) =>
        new(DomainText.Required(value, "University name", minLength: 3, MaxLength));

    internal static UniversityName FromStorage(string value) => Create(value);
}
