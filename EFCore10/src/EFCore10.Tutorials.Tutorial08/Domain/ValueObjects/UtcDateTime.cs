namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record UtcDateTime
{
    private UtcDateTime(DateTime value) => Value = value;

    public DateTime Value { get; }

    internal static UtcDateTime Create(DateTime value) =>
        value.Kind == DateTimeKind.Utc
            ? new UtcDateTime(value)
            : throw new DomainException(DomainErrors.UtcRequired, "Domain timestamps must be UTC.");

    internal static UtcDateTime FromStorage(DateTime value) =>
        Create(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
