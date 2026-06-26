namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record EmployeeStatus
{
    private EmployeeStatus(string value) => Value = value;

    public string Value { get; }

    internal static EmployeeStatus Active => new("Active");

    internal static EmployeeStatus Dismissed => new("Dismissed");

    internal static EmployeeStatus Create(string value) => FromStorage(value);

    internal static EmployeeStatus FromStorage(string value) =>
        value is "Active" or "Dismissed"
            ? new EmployeeStatus(value)
            : throw new DomainException(DomainErrors.EmployeeStatusInvalid, "Employee status is invalid.");
}
