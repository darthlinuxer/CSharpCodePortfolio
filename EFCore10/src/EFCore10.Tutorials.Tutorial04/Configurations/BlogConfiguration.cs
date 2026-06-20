using EFCore10.Tutorials.Tutorial04.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial04.Configurations;

public sealed class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.ToTable("Blogs");

        builder.HasKey(blog => blog.BlogId);

        builder.Property(blog => blog.BlogId)
            .ValueGeneratedOnAdd();

        builder.Property(blog => blog.TenantId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(blog => blog.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(blog => new { blog.TenantId, blog.Url });

        builder.HasMany(blog => blog.Posts)
            .WithOne(post => post.Blog)
            .HasForeignKey(post => post.BlogId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
