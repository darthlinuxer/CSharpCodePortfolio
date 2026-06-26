namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record StudentId
{
    private StudentId(Guid value) => Value = value;

    public Guid Value { get; }

    internal static StudentId New() => FromStorage(Guid.CreateVersion7());

    internal static StudentId Create(Guid value) => FromStorage(value);

    internal static StudentId FromStorage(Guid value) =>
        value == Guid.Empty
            ? throw new DomainException(DomainErrors.StudentIdInvalid, "Student ID cannot be empty.")
            : new StudentId(value);
}
