using EFCore10.Tutorials.Tutorial06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial06.Configurations;

public sealed class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.ToTable("Blogs");

        builder.HasKey(blog => blog.Id);

        builder.Property(blog => blog.Id)
            .HasConversion(id => id.Value, value => new BlogId(value))
            .ValueGeneratedNever();

        builder.Ignore(blog => blog.DomainEvents);

        builder.Property(blog => blog.Name)
            .HasConversion(name => name.Value, value => BlogName.Create(value))
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(blog => blog.Url)
            .HasConversion(url => url.Value, value => BlogUrl.Create(value))
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(blog => blog.AuthorId)
            .HasConversion(id => id.Value, value => new PersonId(value))
            .IsRequired();

        builder.HasIndex(blog => blog.Url);

        builder.HasOne(blog => blog.Author)
            .WithMany()
            .HasForeignKey(blog => blog.AuthorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasMany(blog => blog.Posts)
            .WithOne(post => post.Blog)
            .HasForeignKey(post => post.BlogId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Navigation(blog => blog.Posts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
