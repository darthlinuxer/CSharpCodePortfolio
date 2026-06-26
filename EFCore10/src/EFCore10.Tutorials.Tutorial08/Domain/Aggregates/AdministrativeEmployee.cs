namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed class AdministrativeEmployee : Employee
{
    private AdministrativeEmployee()
    {
    }

    internal AdministrativeEmployee(
        PersonName name,
        EmailAddress email,
        University university,
        StaffRole role,
        UtcDateTime hiredAtUtc)
        : base(name, email, university, hiredAtUtc)
    {
        Role = role;
    }

    public StaffRole Role { get; private set; }
}
