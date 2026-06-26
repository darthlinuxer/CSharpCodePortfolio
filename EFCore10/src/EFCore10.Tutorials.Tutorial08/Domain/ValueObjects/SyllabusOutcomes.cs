namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record SyllabusOutcomes
{
    private SyllabusOutcomes(string value) => Value = value;

    public const int MaxLength = 300;

    public string Value { get; }

    internal static SyllabusOutcomes Create(string? value) =>
        new(DomainText.Required(value, "Syllabus outcomes", minLength: 10, MaxLength));

    internal static SyllabusOutcomes FromStorage(string value) => Create(value);
}
