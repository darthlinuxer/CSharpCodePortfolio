using System.Collections.Frozen;
using System.Reflection;

namespace EFCore10.Tutorials.Tutorial06.Models;

public static class BlogStateRegistry
{
    private static readonly FrozenDictionary<string, Type> StateTypes = DiscoverStates();

    public static BlogState FromKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("Blog state key is required.");

        return StateTypes.TryGetValue(key, out var stateType)
            ? CreateState(stateType)
            : throw new DomainException($"Blog state '{key}' is not supported.");
    }

    private static FrozenDictionary<string, Type> DiscoverStates()
    {
        var stateTypes = typeof(BlogState).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(BlogState).IsAssignableFrom(type))
            .Select(type => (State: CreateState(type), Type: type));

        return stateTypes.ToFrozenDictionary(item => item.State.Key, item => item.Type);
    }

    private static BlogState CreateState(Type stateType) =>
        Activator.CreateInstance(stateType) as BlogState
        ?? throw new DomainException($"Blog state {stateType.GetTypeInfo().Name} must have a parameterless constructor.");
}
