using System.Collections.Frozen;
using System.Reflection;

namespace EFCore10.Tutorials.Tutorial06.Models;

public static class BlogMembershipRoleRegistry
{
    private static readonly FrozenDictionary<string, BlogMembershipRole> Roles = DiscoverRoles();

    public static BlogMembershipRole FromKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("Blog membership role key is required.");

        return Roles.TryGetValue(key, out var role)
            ? role
            : throw new DomainException($"Blog membership role '{key}' is not supported.");
    }

    private static FrozenDictionary<string, BlogMembershipRole> DiscoverRoles()
    {
        var roleTypes = typeof(BlogMembershipRole).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(BlogMembershipRole).IsAssignableFrom(type));

        return roleTypes
            .Select(CreateRole)
            .ToFrozenDictionary(role => role.Key, StringComparer.OrdinalIgnoreCase);
    }

    private static BlogMembershipRole CreateRole(Type roleType) =>
        Activator.CreateInstance(roleType) as BlogMembershipRole
        ?? throw new DomainException($"Blog membership role {roleType.GetTypeInfo().Name} must have a parameterless constructor.");
}
