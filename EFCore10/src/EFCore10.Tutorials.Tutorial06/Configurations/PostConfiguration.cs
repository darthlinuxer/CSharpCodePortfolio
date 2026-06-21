using EFCore10.Tutorials.Tutorial06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial06.Configurations;

public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");

        builder.HasKey(post => post.Id);

        builder.Property(post => post.Id)
            .HasConversion(id => id.Value, value => new PostId(value))
            .ValueGeneratedNever();

        builder.Ignore(post => post.DomainEvents);
        builder.Ignore(post => post.StateName);

        builder.Property(post => post.Title)
            .HasConversion(title => title.Value, value => PostTitle.Create(value))
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(post => post.Content)
            .HasConversion(content => content.Value, value => PostContent.Create(value))
            .IsRequired()
            .HasMaxLength(4_000);

        builder.Property(post => post.BlogId)
            .HasConversion(id => id.Value, value => new BlogId(value))
            .IsRequired();

        builder.Property<string>("StateKey")
            .HasColumnName("PostState")
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(post => post.BlogId);
    }
}
