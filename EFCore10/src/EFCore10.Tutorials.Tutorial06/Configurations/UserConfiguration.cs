using EFCore10.Tutorials.Tutorial06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial06.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasUserIdConversion()
            .ValueGeneratedNever();

        builder.Ignore(user => user.DomainEvents);
        builder.Ignore(user => user.CanLogin);
        builder.Ignore(user => user.UserStateName);

        builder.Property(user => user.Name)
            .HasConversion(name => name.Value, value => PersonName.Create(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.Document)
            .HasConversion(document => document.Value, value => Cpf.Create(value))
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(user => user.UserName)
            .HasConversion(userName => userName.Value, value => UserName.Create(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(user => user.UserName)
            .IsUnique();

        builder.Property(user => user.PasswordHash)
            .HasConversion(passwordHash => passwordHash.Value, value => PasswordHash.FromEncodedHash(value))
            .HasMaxLength(500)
            .IsRequired();

        builder.Property<string>("StateKey")
            .HasColumnName("UserState")
            .HasMaxLength(32)
            .IsRequired();

        builder.ComplexProperty(user => user.Address, address =>
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

        builder.ComplexProperty(user => user.Contact, contact =>
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
