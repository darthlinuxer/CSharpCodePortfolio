namespace EFCore10.Tutorials.Tutorial08.Domain.CourseCatalog;

internal sealed record CourseId
{
    private CourseId(Guid value) => Value = value;

    public Guid Value { get; }

    internal static CourseId New() => new(Guid.CreateVersion7());

    internal static Result<CourseId> Create(Guid value) => FromStorage(value);

    internal static Result<CourseId> FromStorage(Guid value) =>
        value == Guid.Empty
            ? Result<CourseId>.Failure(DomainErrors.CourseIdInvalid)
            : Result<CourseId>.Success(new CourseId(value));
}
