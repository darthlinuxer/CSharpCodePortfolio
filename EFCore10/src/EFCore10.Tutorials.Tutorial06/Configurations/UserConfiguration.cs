using EFCore10.Tutorials.Tutorial06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial06.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Ignore(user => user.CanLogin);
        builder.Ignore(user => user.UserStateName);

        builder.Property(user => user.UserName)
            .HasConversion(userName => userName.Value, value => UserName.Create(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasConversion(passwordHash => passwordHash.Value, value => PasswordHash.FromEncodedHash(value))
            .HasMaxLength(500)
            .IsRequired();

        builder.Property<string>("StateKey")
            .HasColumnName("UserState")
            .HasMaxLength(32)
            .IsRequired();
    }
}
