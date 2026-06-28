namespace EFCore10.Tutorials.Tutorial08.Domain.CourseCatalog;

internal sealed record SyllabusOutcomes
{
    private SyllabusOutcomes(string value) => Value = value;

    public const int MaxLength = 300;

    public string Value { get; }

    internal static Result<SyllabusOutcomes> Create(string? value)
    {
        var text = DomainText.Required(value, "Syllabus outcomes", minLength: 10, MaxLength);

        return text.IsSuccess
            ? Result<SyllabusOutcomes>.Success(new SyllabusOutcomes(text.RequireValue()))
            : Result<SyllabusOutcomes>.Failure(text.Errors);
    }

    internal static Result<SyllabusOutcomes> FromStorage(string value) => Create(value);
}
