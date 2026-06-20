using EFCore10.Tutorials.Tutorial03.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial03.Configurations;

public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");

        builder.HasKey(post => post.PostId);

        builder.Property(post => post.PostId)
            .ValueGeneratedOnAdd();

        builder.Property(post => post.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(post => post.Content)
            .IsRequired()
            .HasMaxLength(4_000);

        builder.HasIndex(post => post.BlogId);
    }
}
