namespace EFCore10.Tutorials.Tutorial08.Domain.Institutional;

internal sealed record EmployeeId
{
    private EmployeeId(Guid value) => Value = value;

    public Guid Value { get; }

    internal static EmployeeId New() => new(Guid.CreateVersion7());

    internal static Result<EmployeeId> Create(Guid value) => FromStorage(value);

    internal static Result<EmployeeId> FromStorage(Guid value) =>
        value == Guid.Empty
            ? Result<EmployeeId>.Failure(DomainErrors.EmployeeIdInvalid)
            : Result<EmployeeId>.Success(new EmployeeId(value));
}
