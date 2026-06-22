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
            .HasBlogIdConversion()
            .ValueGeneratedNever();

        builder.Ignore(blog => blog.DomainEvents);
        builder.Ignore(blog => blog.Authors);

        builder.Property(blog => blog.Name)
            .HasConversion(name => name.Value, value => BlogName.Create(value))
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(blog => blog.Url)
            .HasConversion(url => url.Value, value => BlogUrl.Create(value))
            .IsRequired()
            .HasMaxLength(500);

        builder.Ignore(blog => blog.CurrentOwner);
        builder.Ignore(blog => blog.StateName);

        builder.Property<string>("StateKey")
            .HasColumnName("BlogState")
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(blog => blog.Url);

        builder.HasMany(blog => blog.Posts)
            .WithOne(post => post.Blog)
            .HasForeignKey(post => post.BlogId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Navigation(blog => blog.Posts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(blog => blog.Memberships)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
