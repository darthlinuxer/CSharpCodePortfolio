namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct CourseTitle(string Value)
{
    public const int MaxLength = 160;

    internal static CourseTitle Create(string? value) =>
        new(DomainText.Required(value, "Course title", minLength: 3, MaxLength));
}
