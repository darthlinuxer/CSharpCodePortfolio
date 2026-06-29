using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence.ConfigurationMappings;

/// <summary>
/// Maps the DDD aggregate directly, using EF Core complex properties for
/// required value objects and a <see cref="ValueConverter"/> to translate
/// the optional <c>Option&lt;PhoneNumber&gt;</c> phone into a nullable
/// relational column.
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

        var phoneConverter = new ValueConverter<Option<PhoneNumber>, string?>(
            convertToProviderExpression: option => option.Match(Some: p => p.Value, None: () => null),
            convertFromProviderExpression: value => value is null
                ? Option<PhoneNumber>.None
                : Some(new PhoneNumber(value)));

        builder.Property(user => user.PhoneNumber)
            .HasColumnName("PhoneNumber")
            .HasMaxLength(15)
            .HasConversion(phoneConverter);
    }
}