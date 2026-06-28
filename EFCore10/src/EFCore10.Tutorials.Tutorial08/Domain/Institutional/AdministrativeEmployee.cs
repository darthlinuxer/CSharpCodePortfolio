namespace EFCore10.Tutorials.Tutorial08.Domain.Institutional;

internal sealed class AdministrativeEmployee : Employee
{
    private AdministrativeEmployee()
    {
    }

    private AdministrativeEmployee(
        PersonName name,
        EmailAddress email,
        University university,
        StaffRole role,
        UtcDateTime hiredAtUtc)
        : base(name, email, university, hiredAtUtc)
    {
        Role = role;
    }

    public StaffRole Role { get; private set; } = null!;

    internal static AdministrativeEmployee Create(
        PersonName name,
        EmailAddress email,
        University university,
        StaffRole role,
        UtcDateTime hiredAtUtc) =>
        new(name, email, university, role, hiredAtUtc);
}
