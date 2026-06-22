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
            .HasConversion(id => id.Value, value => PostId.From(value))
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
            .HasConversion(id => id.Value, value => BlogId.From(value))
            .IsRequired();

        builder.Property(post => post.PostedByUserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(post => post.CreatedOnUtc)
            .HasConversion(timestamp => timestamp.Value, value => Timestamp.FromDatabase(value))
            .IsRequired();

        builder.Property(post => post.PublishedOnUtc)
            .HasConversion(
                timestamp => timestamp.HasValue ? timestamp.Value.Value : (DateTime?)null,
                value => value.HasValue ? Timestamp.FromDatabase(value.Value) : null);

        builder.Property(post => post.ArchivedOnUtc)
            .HasConversion(
                timestamp => timestamp.HasValue ? timestamp.Value.Value : (DateTime?)null,
                value => value.HasValue ? Timestamp.FromDatabase(value.Value) : null);

        builder.Property<string>("StateKey")
            .HasColumnName("PostState")
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(post => post.BlogId);

        builder.HasIndex(post => new { post.BlogId, post.PublishedOnUtc });

        builder.HasIndex(post => post.PostedByUserId);

        builder.HasOne(post => post.PostedBy)
            .WithMany()
            .HasForeignKey(post => post.PostedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
