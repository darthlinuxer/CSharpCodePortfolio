namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record CourseTitle
{
    private CourseTitle(string value) => Value = value;

    public const int MaxLength = 160;

    public string Value { get; }

    internal static CourseTitle Create(string? value) =>
        new(DomainText.Required(value, "Course title", minLength: 3, MaxLength));

    internal static CourseTitle FromStorage(string value) => Create(value);
}
