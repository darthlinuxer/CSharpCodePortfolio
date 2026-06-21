namespace EFCore10.Tutorials.Tutorial06.Models;

public abstract class Person : AggregateRoot<PersonId>
{
    protected Person()
    {
    }

    protected Person(PersonName name, Cpf document, Address address, Contact contact)
    {
        Id = PersonId.NewId();
        Name = name;
        Document = document;
        Address = address;
        Contact = contact;
    }

    public PersonName Name { get; protected set; } = null!;

    public Cpf Document { get; protected set; } = null!;

    public Address Address { get; protected set; } = null!;

    public Contact Contact { get; protected set; } = null!;

    public void ChangeAddress(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);
        Address = address;
    }

    public void ChangeContact(Contact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);
        Contact = contact;
    }
}
