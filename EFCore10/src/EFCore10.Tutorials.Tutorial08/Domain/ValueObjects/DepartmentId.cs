namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record DepartmentId
{
    private DepartmentId(Guid value) => Value = value;

    public Guid Value { get; }

    internal static DepartmentId New() => FromStorage(Guid.CreateVersion7());

    internal static DepartmentId Create(Guid value) => FromStorage(value);

    internal static DepartmentId FromStorage(Guid value) =>
        value == Guid.Empty
            ? throw new DomainException(DomainErrors.DepartmentIdInvalid, "Department ID cannot be empty.")
            : new DepartmentId(value);
}
