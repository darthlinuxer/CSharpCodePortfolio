namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct CourseCode(string Value)
{
    public const int MaxLength = 20;

    internal static CourseCode Create(string? value) =>
        new(DomainText.Required(value, "Course code", minLength: 2, MaxLength).ToUpperInvariant());
}
