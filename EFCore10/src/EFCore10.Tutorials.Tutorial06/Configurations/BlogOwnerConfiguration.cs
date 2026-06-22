using EFCore10.Tutorials.Tutorial06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial06.Configurations;

public sealed class BlogOwnerConfiguration : IEntityTypeConfiguration<BlogOwner>
{
    public void Configure(EntityTypeBuilder<BlogOwner> builder)
    {
        builder.ToTable("BlogOwners");

        builder.HasKey(owner => owner.Id);

        builder.Property(owner => owner.Id)
            .HasConversion(id => id.Value, value => BlogOwnerId.From(value))
            .ValueGeneratedNever();

        builder.Property(owner => owner.BlogId)
            .HasConversion(id => id.Value, value => BlogId.From(value))
            .IsRequired();

        builder.Property(owner => owner.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(owner => owner.StartedOnUtc)
            .IsRequired();

        builder.Property(owner => owner.EndedOnUtc);

        builder.Ignore(owner => owner.IsActive);

        builder.HasIndex(owner => owner.BlogId)
            .HasFilter("EndedOnUtc IS NULL")
            .IsUnique();

        builder.HasOne(owner => owner.Blog)
            .WithMany(blog => blog.Owners)
            .HasForeignKey(owner => owner.BlogId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(owner => owner.User)
            .WithMany()
            .HasForeignKey(owner => owner.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
