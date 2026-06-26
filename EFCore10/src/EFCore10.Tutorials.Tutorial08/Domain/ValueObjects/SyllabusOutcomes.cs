namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct SyllabusOutcomes(string Value)
{
    public const int MaxLength = 300;

    internal static SyllabusOutcomes Create(string? value) =>
        new(DomainText.Required(value, "Syllabus outcomes", minLength: 10, MaxLength));
}
