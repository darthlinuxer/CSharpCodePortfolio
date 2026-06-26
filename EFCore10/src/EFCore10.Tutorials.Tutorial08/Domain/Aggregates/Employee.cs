namespace EFCore10.Tutorials.Tutorial08.Domain;

internal abstract class Employee : DomainEntity<EmployeeId>
{
    protected Employee()
    {
    }

    protected Employee(PersonName name, EmailAddress email, University university, UtcDateTime hiredAtUtc)
        : base(EmployeeId.New())
    {
        ArgumentNullException.ThrowIfNull(university);

        Name = name;
        Email = email;
        University = university;
        HiredAtUtc = hiredAtUtc;
        Status = EmployeeStatus.Active;
    }

    public PersonName Name { get; private set; } = null!;

    public EmailAddress Email { get; private set; } = null!;

    public University University { get; private set; } = null!;

    public UtcDateTime HiredAtUtc { get; private set; } = null!;

    public UtcDateTime? DismissedAtUtc { get; private set; }

    public EmployeeStatus Status { get; private set; } = null!;

    /// <summary>
    /// Marks the employee as dismissed.
    /// </summary>
    internal IReadOnlyList<DomainError> GetDismissalErrors(UtcDateTime dismissedAtUtc)
    {
        var errors = new List<DomainError>();

        if (Status == EmployeeStatus.Dismissed)
            errors.Add(DomainErrors.EmployeeAlreadyDismissed);
        if (dismissedAtUtc.Value < HiredAtUtc.Value)
            errors.Add(DomainErrors.EmployeeDismissalDateInvalid);

        return errors;
    }

    internal Result Dismiss(UtcDateTime dismissedAtUtc)
    {
        var errors = GetDismissalErrors(dismissedAtUtc);

        if (errors is not [])
            return Result.Failure(errors);

        DismissedAtUtc = dismissedAtUtc;
        Status = EmployeeStatus.Dismissed;

        return Result.Success();
    }
}
