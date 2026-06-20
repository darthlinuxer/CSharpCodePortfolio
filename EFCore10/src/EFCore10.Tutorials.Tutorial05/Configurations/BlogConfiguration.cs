using EFCore10.Tutorials.Tutorial05.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial05.Configurations;

public sealed class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.ToTable("Blogs");

        builder.HasKey(blog => blog.BlogId);

        builder.Property(blog => blog.BlogId)
            .ValueGeneratedOnAdd();

        builder.Property(blog => blog.Url)
            .IsRequired()
            .HasMaxLength(500);
    }
}
