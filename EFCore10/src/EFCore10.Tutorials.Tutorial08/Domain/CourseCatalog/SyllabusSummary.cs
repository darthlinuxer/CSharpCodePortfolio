namespace EFCore10.Tutorials.Tutorial08.Domain.CourseCatalog;

internal sealed record SyllabusSummary
{
    private SyllabusSummary(string value) => Value = value;

    public const int MaxLength = 240;

    public string Value { get; }

    internal static Result<SyllabusSummary> Create(string? value)
    {
        var text = DomainText.Required(value, "Syllabus summary", minLength: 10, MaxLength);

        return text.IsSuccess
            ? Result<SyllabusSummary>.Success(new SyllabusSummary(text.RequireValue()))
            : Result<SyllabusSummary>.Failure(text.Errors);
    }

    internal static Result<SyllabusSummary> FromStorage(string value) => Create(value);
}
