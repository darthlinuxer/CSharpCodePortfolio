using System.Reflection;

namespace EFCore10.Tutorials.Tutorial06.Models;

public static class PostStateRegistry
{
    private static readonly Lazy<IReadOnlyDictionary<string, Type>> StateTypes = new(LoadStateTypes);

    public static PostState FromKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("Post state key is required.");

        return StateTypes.Value.TryGetValue(key, out var stateType)
            ? CreateState(stateType)
            : throw new DomainException($"Invalid post state: {key}.");
    }

    private static IReadOnlyDictionary<string, Type> LoadStateTypes()
    {
        var stateTypes = typeof(PostState).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(PostState).IsAssignableFrom(type))
            .Select(type => (Type: type, State: CreateState(type)))
            .ToArray();

        return stateTypes.ToDictionary(
            item => item.State.Key,
            item => item.Type,
            StringComparer.Ordinal);
    }

    private static PostState CreateState(Type stateType) =>
        Activator.CreateInstance(stateType) as PostState
        ?? throw new DomainException($"Post state {stateType.GetTypeInfo().Name} must have a parameterless constructor.");
}
