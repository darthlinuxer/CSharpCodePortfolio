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
            .HasPostIdConversion()
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
            .HasBlogIdConversion()
            .IsRequired();

        builder.Property(post => post.PostedByUserId)
            .HasUserIdConversion()
            .IsRequired();

        builder.Property(post => post.CreatedOnUtc)
            .HasTimestampConversion()
            .IsRequired();

        builder.Property(post => post.PublishedOnUtc)
            .HasNullableTimestampConversion();

        builder.Property(post => post.ArchivedOnUtc)
            .HasNullableTimestampConversion();

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
