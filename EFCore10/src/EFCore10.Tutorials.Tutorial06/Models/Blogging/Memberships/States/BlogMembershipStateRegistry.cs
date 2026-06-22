using System.Collections.Frozen;
using System.Reflection;

namespace EFCore10.Tutorials.Tutorial06.Models;

public static class BlogMembershipStateRegistry
{
    private static readonly FrozenDictionary<string, BlogMembershipState> States = DiscoverStates();

    public static BlogMembershipState FromKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("Blog membership state key is required.");

        return States.TryGetValue(key, out var state)
            ? state
            : throw new DomainException($"Blog membership state '{key}' is not supported.");
    }

    private static FrozenDictionary<string, BlogMembershipState> DiscoverStates()
    {
        var stateTypes = typeof(BlogMembershipState).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(BlogMembershipState).IsAssignableFrom(type));

        return stateTypes
            .Select(CreateState)
            .ToFrozenDictionary(state => state.Key, StringComparer.OrdinalIgnoreCase);
    }

    private static BlogMembershipState CreateState(Type stateType) =>
        Activator.CreateInstance(stateType) as BlogMembershipState
        ?? throw new DomainException($"Blog membership state {stateType.GetTypeInfo().Name} must have a parameterless constructor.");
}
