namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record SyllabusSummary
{
    private SyllabusSummary(string value) => Value = value;

    public const int MaxLength = 240;

    public string Value { get; }

    internal static SyllabusSummary Create(string? value) =>
        new(DomainText.Required(value, "Syllabus summary", minLength: 10, MaxLength));

    internal static SyllabusSummary FromStorage(string value) => Create(value);
}
