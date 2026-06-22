using System.Collections.Frozen;
using System.Reflection;

namespace EFCore10.Tutorials.Tutorial06.Models;

public static class AuthorInvitationStateRegistry
{
    private static readonly FrozenDictionary<string, Type> StateTypes = DiscoverStates();

    public static AuthorInvitationState FromKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("Author invitation state key is required.");

        return StateTypes.TryGetValue(key, out var stateType)
            ? CreateState(stateType)
            : throw new DomainException($"Author invitation state '{key}' is not supported.");
    }

    private static FrozenDictionary<string, Type> DiscoverStates()
    {
        var stateTypes = typeof(AuthorInvitationState).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(AuthorInvitationState).IsAssignableFrom(type))
            .Select(type => (State: CreateState(type), Type: type));

        return stateTypes.ToFrozenDictionary(item => item.State.Key, item => item.Type);
    }

    private static AuthorInvitationState CreateState(Type stateType) =>
        Activator.CreateInstance(stateType) as AuthorInvitationState
        ?? throw new DomainException($"Author invitation state {stateType.GetTypeInfo().Name} must have a parameterless constructor.");
}
