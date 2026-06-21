using EFCore10.Tutorials.Tutorial06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial06.Configurations;

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("People");

        builder.UseTphMappingStrategy();

        builder.HasKey(person => person.Id);

        builder.Property(person => person.Id)
            .HasConversion(id => id.Value, value => new PersonId(value))
            .ValueGeneratedNever();

        builder.HasDiscriminator<string>("PersonType")
            .HasValue<Author>("Author");

        builder.Ignore(person => person.DomainEvents);

        builder.Property(person => person.Name)
            .HasConversion(name => name.Value, value => PersonName.Create(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(person => person.Document)
            .HasConversion(document => document.Value, value => Cpf.Create(value))
            .HasMaxLength(11)
            .IsRequired();

        builder.ComplexProperty(person => person.Address, address =>
        {
            address.Property(value => value.Street)
                .HasColumnName("Street")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(value => value.Number)
                .HasColumnName("Number")
                .HasMaxLength(30)
                .IsRequired();

            address.Property(value => value.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(value => value.State)
                .HasConversion(state => state.Value, value => StateCode.Create(value))
                .HasColumnName("State")
                .HasMaxLength(2)
                .IsRequired();

            address.Property(value => value.ZipCode)
                .HasConversion(zipCode => zipCode.Value, value => ZipCode.Create(value))
                .HasColumnName("ZipCode")
                .HasMaxLength(8)
                .IsRequired();
        });

        builder.ComplexProperty(person => person.Contact, contact =>
        {
            contact.Property(value => value.Email)
                .HasConversion(email => email.Value, value => Email.Create(value))
                .HasColumnName("Email")
                .HasMaxLength(200)
                .IsRequired();

            contact.Property(value => value.Phone)
                .HasConversion(phone => phone.Value, value => PhoneNumber.Create(value))
                .HasColumnName("Phone")
                .HasMaxLength(20)
                .IsRequired();
        });
    }
}
