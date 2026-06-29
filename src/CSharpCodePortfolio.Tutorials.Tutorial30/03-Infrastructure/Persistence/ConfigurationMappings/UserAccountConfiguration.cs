using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence.ConfigurationMappings;

/// <summary>
/// Maps the DDD aggregate directly, using EF Core complex properties for value objects.
/// </summary>
public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    /// <summary>
    /// Configures the aggregate table without introducing a persistence-only record type.
    /// </summary>
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Ignore(user => user.DomainEvents);
        builder.Ignore(user => user.CreatedAt);
        builder.Ignore(user => user.LastModified);
        builder.Ignore(user => user.PhoneNumber);

        builder.ComplexProperty(user => user.Name, name =>
        {
            name.Property(value => value.Value)
                .HasColumnName("Name")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.ComplexProperty(user => user.Email, email =>
        {
            email.Property(value => value.Value)
                .HasColumnName("Email")
                .HasMaxLength(320)
                .IsRequired();
        });

        builder.ComplexProperty(user => user.PhoneNumberValue, phone =>
        {
            phone.Property(value => value.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(15);
        });
    }
}