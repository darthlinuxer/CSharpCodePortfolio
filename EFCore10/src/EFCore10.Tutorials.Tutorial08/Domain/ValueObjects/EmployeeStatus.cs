namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct EmployeeStatus(string Value)
{
    internal static EmployeeStatus Active => new("Active");

    internal static EmployeeStatus Dismissed => new("Dismissed");

    internal static EmployeeStatus FromStorage(string value) =>
        value is "Active" or "Dismissed"
            ? new EmployeeStatus(value)
            : throw new DomainException(DomainErrors.EmployeeStatusInvalid, "Employee status is invalid.");
}
