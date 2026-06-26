namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct UtcDateTime(DateTime Value)
{
    internal static UtcDateTime Create(DateTime value) =>
        value.Kind == DateTimeKind.Utc
            ? new UtcDateTime(value)
            : throw new DomainException(DomainErrors.UtcRequired, "Domain timestamps must be UTC.");
}
