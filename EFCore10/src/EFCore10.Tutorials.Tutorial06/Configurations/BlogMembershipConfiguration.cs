using EFCore10.Tutorials.Tutorial06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial06.Configurations;

public sealed class BlogMembershipConfiguration : IEntityTypeConfiguration<BlogMembership>
{
    public void Configure(EntityTypeBuilder<BlogMembership> builder)
    {
        builder.ToTable("BlogMemberships");

        builder.HasKey(membership => membership.Id);

        builder.Property(membership => membership.Id)
            .HasConversion(id => id.Value, value => BlogMembershipId.From(value))
            .ValueGeneratedNever();

        builder.Property(membership => membership.BlogId)
            .HasConversion(id => id.Value, value => BlogId.From(value))
            .IsRequired();

        builder.Property(membership => membership.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(membership => membership.CreatedByUserId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? UserId.From(value.Value) : null);

        builder.Property(membership => membership.EndedByUserId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? UserId.From(value.Value) : null);

        builder.Property(membership => membership.CreatedOnUtc)
            .HasConversion(timestamp => timestamp.Value, value => Timestamp.FromDatabase(value))
            .IsRequired();

        builder.Property(membership => membership.ActivatedOnUtc)
            .HasConversion(
                timestamp => timestamp.HasValue ? timestamp.Value.Value : (DateTime?)null,
                value => value.HasValue ? Timestamp.FromDatabase(value.Value) : null);

        builder.Property(membership => membership.EndedOnUtc)
            .HasConversion(
                timestamp => timestamp.HasValue ? timestamp.Value.Value : (DateTime?)null,
                value => value.HasValue ? Timestamp.FromDatabase(value.Value) : null);

        builder.Ignore(membership => membership.RoleName);
        builder.Ignore(membership => membership.StateName);
        builder.Ignore(membership => membership.IsActive);
        builder.Ignore(membership => membership.CanPost);

        builder.Property<string>("RoleKey")
            .HasColumnName("MembershipRole")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property<string>("StateKey")
            .HasColumnName("MembershipState")
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(membership => membership.BlogId)
            .HasFilter("MembershipRole = 'Owner' AND MembershipState = 'Active'")
            .IsUnique();

        builder.HasIndex(membership => new { membership.BlogId, membership.UserId })
            .HasFilter("MembershipState IN ('Pending', 'Active')");

        builder.HasOne(membership => membership.Blog)
            .WithMany(blog => blog.Memberships)
            .HasForeignKey(membership => membership.BlogId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(membership => membership.User)
            .WithMany()
            .HasForeignKey(membership => membership.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
