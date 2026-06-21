namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record Contact
{
    private Contact()
    {
    }

    private Contact(Email email, PhoneNumber phone)
    {
        Email = email;
        Phone = phone;
    }

    public Email Email { get; private set; } = null!;

    public PhoneNumber Phone { get; private set; } = null!;

    public static Contact Create(Email email, PhoneNumber phone)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(phone);

        return new Contact(email, phone);
    }
}
