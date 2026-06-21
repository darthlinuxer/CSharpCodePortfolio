namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class Author : User
{
    private Author()
    {
    }

    private Author(
        PersonName name,
        Cpf document,
        Address address,
        Contact contact,
        UserName userName,
        PasswordHash passwordHash)
        : base(name, document, address, contact, userName, passwordHash)
    {
    }

    public AuthorId AuthorId => AuthorId.From(Id);

    public static Author Create(
        PersonName name,
        Cpf document,
        Address address,
        Contact contact,
        UserName userName,
        PasswordHash passwordHash)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(userName);
        ArgumentNullException.ThrowIfNull(passwordHash);

        var author = new Author(name, document, address, contact, userName, passwordHash);
        author.Raise(new AuthorCreatedDomainEvent(author.AuthorId, DateTime.UtcNow));

        return author;
    }
}
