namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record CourseCode
{
    private CourseCode(string value) => Value = value;

    public const int MaxLength = 20;

    public string Value { get; }

    internal static Result<CourseCode> Create(string? value)
    {
        var text = DomainText.Required(value, "Course code", minLength: 2, MaxLength);

        return text.IsSuccess
            ? Result<CourseCode>.Success(new CourseCode(text.RequireValue().ToUpperInvariant()))
            : Result<CourseCode>.Failure(text.Errors);
    }

    internal static Result<CourseCode> FromStorage(string value) => Create(value);
}
