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

    public PersonName Name { get; private set; }

    public EmailAddress Email { get; private set; }

    public University University { get; private set; } = null!;

    public UtcDateTime HiredAtUtc { get; private set; }

    public UtcDateTime? DismissedAtUtc { get; private set; }

    public EmployeeStatus Status { get; private set; }

    /// <summary>
    /// Marks the employee as dismissed.
    /// </summary>
    public void Dismiss(UtcDateTime dismissedAtUtc)
    {
        if (Status == EmployeeStatus.Dismissed)
            throw new DomainException(DomainErrors.EmployeeAlreadyDismissed, "Employee is already dismissed.");
        if (dismissedAtUtc.Value < HiredAtUtc.Value)
            throw new DomainException(DomainErrors.EmployeeDismissalDateInvalid, "Dismissal date cannot be earlier than hiring date.");

        DismissedAtUtc = dismissedAtUtc;
        Status = EmployeeStatus.Dismissed;
    }
}
