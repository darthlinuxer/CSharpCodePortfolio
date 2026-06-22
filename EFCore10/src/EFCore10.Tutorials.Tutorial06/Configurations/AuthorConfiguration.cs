using EFCore10.Tutorials.Tutorial06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial06.Configurations;

public sealed class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors");

        builder.HasKey(author => author.Id);

        builder.Property(author => author.Id)
            .HasConversion(id => id.Value, value => AuthorId.From(value))
            .ValueGeneratedNever();

        builder.Property(author => author.BlogId)
            .HasConversion(id => id.Value, value => BlogId.From(value))
            .IsRequired();

        builder.Property(author => author.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Ignore(author => author.StateName);
        builder.Ignore(author => author.CanPost);

        builder.Property<string>("StateKey")
            .HasColumnName("AuthorState")
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(author => new { author.BlogId, author.UserId })
            .IsUnique();

        builder.HasOne(author => author.Blog)
            .WithMany(blog => blog.Authors)
            .HasForeignKey(author => author.BlogId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(author => author.User)
            .WithMany()
            .HasForeignKey(author => author.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
