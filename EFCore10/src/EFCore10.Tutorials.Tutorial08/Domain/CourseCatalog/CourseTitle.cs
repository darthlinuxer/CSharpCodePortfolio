namespace EFCore10.Tutorials.Tutorial08.Domain.CourseCatalog;

internal sealed record CourseTitle
{
    private CourseTitle(string value) => Value = value;

    public const int MaxLength = 160;

    public string Value { get; }

    internal static Result<CourseTitle> Create(string? value)
    {
        var text = DomainText.Required(value, "Course title", minLength: 3, MaxLength);

        return text.IsSuccess
            ? Result<CourseTitle>.Success(new CourseTitle(text.RequireValue()))
            : Result<CourseTitle>.Failure(text.Errors);
    }

    internal static Result<CourseTitle> FromStorage(string value) => Create(value);
}
