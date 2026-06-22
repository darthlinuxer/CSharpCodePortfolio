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
            .HasBlogMembershipIdConversion()
            .ValueGeneratedNever();

        builder.Property(membership => membership.BlogId)
            .HasBlogIdConversion()
            .IsRequired();

        builder.Property(membership => membership.UserId)
            .HasUserIdConversion()
            .IsRequired();

        builder.Property(membership => membership.CreatedByUserId)
            .HasNullableUserIdConversion();

        builder.Property(membership => membership.EndedByUserId)
            .HasNullableUserIdConversion();

        builder.Property(membership => membership.CreatedOnUtc)
            .HasTimestampConversion()
            .IsRequired();

        builder.Property(membership => membership.ActivatedOnUtc)
            .HasNullableTimestampConversion();

        builder.Property(membership => membership.EndedOnUtc)
            .HasNullableTimestampConversion();

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
