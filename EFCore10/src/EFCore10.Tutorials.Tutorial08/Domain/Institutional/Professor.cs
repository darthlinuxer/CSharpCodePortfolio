namespace EFCore10.Tutorials.Tutorial08.Domain.Institutional;

internal sealed class Professor : Employee
{
    private Professor()
    {
    }

    private Professor(
        PersonName name,
        EmailAddress email,
        University university,
        Department department,
        UtcDateTime hiredAtUtc)
        : base(name, email, university, hiredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(department);

        Department = department;
    }

    public Department Department { get; private set; } = null!;

    internal static Professor Create(
        PersonName name,
        EmailAddress email,
        University university,
        Department department,
        UtcDateTime hiredAtUtc) =>
        new(name, email, university, department, hiredAtUtc);

    public ProfessorAssignmentSnapshot ToAssignmentSnapshot() => new(Id, Department.Id, Status);
}
