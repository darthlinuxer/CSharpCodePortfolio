namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record EmployeeId
{
    private EmployeeId(Guid value) => Value = value;

    public Guid Value { get; }

    internal static EmployeeId New() => FromStorage(Guid.CreateVersion7());

    internal static EmployeeId Create(Guid value) => FromStorage(value);

    internal static EmployeeId FromStorage(Guid value) =>
        value == Guid.Empty
            ? throw new DomainException(DomainErrors.EmployeeIdInvalid, "Employee ID cannot be empty.")
            : new EmployeeId(value);
}
