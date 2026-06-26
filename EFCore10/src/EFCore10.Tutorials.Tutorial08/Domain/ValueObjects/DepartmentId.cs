namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record DepartmentId
{
    private DepartmentId(Guid value) => Value = value;

    public Guid Value { get; }

    internal static DepartmentId New() => new(Guid.CreateVersion7());

    internal static Result<DepartmentId> Create(Guid value) => FromStorage(value);

    internal static Result<DepartmentId> FromStorage(Guid value) =>
        value == Guid.Empty
            ? Result<DepartmentId>.Failure(DomainErrors.DepartmentIdInvalid)
            : Result<DepartmentId>.Success(new DepartmentId(value));
}
