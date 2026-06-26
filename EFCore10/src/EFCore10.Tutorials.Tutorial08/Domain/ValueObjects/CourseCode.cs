namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record CourseCode
{
    private CourseCode(string value) => Value = value;

    public const int MaxLength = 20;

    public string Value { get; }

    internal static CourseCode Create(string? value) =>
        new(DomainText.Required(value, "Course code", minLength: 2, MaxLength).ToUpperInvariant());

    internal static CourseCode FromStorage(string value) => Create(value);
}
