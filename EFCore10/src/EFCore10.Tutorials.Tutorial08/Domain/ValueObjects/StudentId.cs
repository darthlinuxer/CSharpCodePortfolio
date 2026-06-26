namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record StudentId
{
    private StudentId(Guid value) => Value = value;

    public Guid Value { get; }

    internal static StudentId New() => new(Guid.CreateVersion7());

    internal static Result<StudentId> Create(Guid value) => FromStorage(value);

    internal static Result<StudentId> FromStorage(Guid value) =>
        value == Guid.Empty
            ? Result<StudentId>.Failure(DomainErrors.StudentIdInvalid)
            : Result<StudentId>.Success(new StudentId(value));
}
