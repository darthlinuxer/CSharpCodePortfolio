namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct SyllabusSummary(string Value)
{
    public const int MaxLength = 240;

    internal static SyllabusSummary Create(string? value) =>
        new(DomainText.Required(value, "Syllabus summary", minLength: 10, MaxLength));
}
