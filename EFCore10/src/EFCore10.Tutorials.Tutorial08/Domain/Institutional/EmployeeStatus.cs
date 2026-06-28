namespace EFCore10.Tutorials.Tutorial08.Domain.Institutional;

internal sealed record EmployeeStatus
{
    private EmployeeStatus(string value) => Value = value;

    public string Value { get; }

    internal static EmployeeStatus Active => new("Active");

    internal static EmployeeStatus Dismissed => new("Dismissed");

    internal static Result<EmployeeStatus> Create(string value) => FromStorage(value);

    internal static Result<EmployeeStatus> FromStorage(string value) =>
        value is "Active" or "Dismissed"
            ? Result<EmployeeStatus>.Success(new EmployeeStatus(value))
            : Result<EmployeeStatus>.Failure(DomainErrors.EmployeeStatusInvalid);
}
