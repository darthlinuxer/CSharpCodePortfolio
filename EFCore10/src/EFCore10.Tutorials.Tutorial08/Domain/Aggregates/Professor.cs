namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed class Professor : Employee
{
    private Professor()
    {
    }

    internal Professor(
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
}
