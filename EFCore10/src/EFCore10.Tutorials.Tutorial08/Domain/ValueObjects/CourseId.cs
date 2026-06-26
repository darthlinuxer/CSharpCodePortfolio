namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record CourseId
{
    private CourseId(Guid value) => Value = value;

    public Guid Value { get; }

    internal static CourseId New() => FromStorage(Guid.CreateVersion7());

    internal static CourseId Create(Guid value) => FromStorage(value);

    internal static CourseId FromStorage(Guid value) =>
        value == Guid.Empty
            ? throw new DomainException(DomainErrors.CourseIdInvalid, "Course ID cannot be empty.")
            : new CourseId(value);
}
