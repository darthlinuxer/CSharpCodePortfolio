namespace EFCore10.Tutorials.Tutorial06.Models;

internal static class StronglyTypedId
{
    public static Guid NewValue() => Guid.CreateVersion7();

    public static Guid Require(Guid value, string valueName) =>
        value == Guid.Empty ? throw new DomainException($"{valueName} is required.") : value;
}
