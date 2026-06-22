using EFCore10.Tutorials.Tutorial06.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial06.Configurations;

internal static class ValueConversionExtensions
{
    public static PropertyBuilder<UserId> HasUserIdConversion(this PropertyBuilder<UserId> builder) =>
        builder.HasConversion(id => id.Value, value => UserId.From(value));

    public static PropertyBuilder<UserId?> HasNullableUserIdConversion(this PropertyBuilder<UserId?> builder) =>
        builder.HasConversion(
            id => id.HasValue ? id.Value.Value : (Guid?)null,
            value => value.HasValue ? UserId.From(value.Value) : (UserId?)null);

    public static PropertyBuilder<BlogId> HasBlogIdConversion(this PropertyBuilder<BlogId> builder) =>
        builder.HasConversion(id => id.Value, value => BlogId.From(value));

    public static PropertyBuilder<BlogMembershipId> HasBlogMembershipIdConversion(
        this PropertyBuilder<BlogMembershipId> builder) =>
        builder.HasConversion(id => id.Value, value => BlogMembershipId.From(value));

    public static PropertyBuilder<PostId> HasPostIdConversion(this PropertyBuilder<PostId> builder) =>
        builder.HasConversion(id => id.Value, value => PostId.From(value));

    public static PropertyBuilder<Timestamp> HasTimestampConversion(this PropertyBuilder<Timestamp> builder) =>
        builder.HasConversion(timestamp => timestamp.Value, value => Timestamp.FromDatabase(value));

    public static PropertyBuilder<Timestamp?> HasNullableTimestampConversion(this PropertyBuilder<Timestamp?> builder) =>
        builder.HasConversion(
            timestamp => timestamp.HasValue ? timestamp.Value.Value : (DateTime?)null,
            value => value.HasValue ? Timestamp.FromDatabase(value.Value) : (Timestamp?)null);
}
