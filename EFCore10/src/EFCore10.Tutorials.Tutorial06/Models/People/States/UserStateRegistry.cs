using System.Reflection;

namespace EFCore10.Tutorials.Tutorial06.Models;

public static class UserStateRegistry
{
    private static readonly Lazy<IReadOnlyDictionary<string, Type>> StateTypes = new(LoadStateTypes);

    public static UserState FromKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("User state key is required.");

        return StateTypes.Value.TryGetValue(key, out var stateType)
            ? CreateState(stateType)
            : throw new DomainException($"Invalid user state: {key}.");
    }

    private static IReadOnlyDictionary<string, Type> LoadStateTypes()
    {
        var stateTypes = typeof(UserState).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(UserState).IsAssignableFrom(type))
            .Select(type => (Type: type, State: CreateState(type)))
            .ToArray();

        return stateTypes.ToDictionary(
            item => item.State.Key,
            item => item.Type,
            StringComparer.Ordinal);
    }

    private static UserState CreateState(Type stateType) =>
        Activator.CreateInstance(stateType) as UserState
        ?? throw new DomainException($"User state {stateType.GetTypeInfo().Name} must have a parameterless constructor.");
}
