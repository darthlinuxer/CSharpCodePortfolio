namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record UtcDateTime
{
    private UtcDateTime(DateTime value) => Value = value;

    public DateTime Value { get; }

    internal static Result<UtcDateTime> Create(DateTime value) =>
        value.Kind == DateTimeKind.Utc
            ? Result<UtcDateTime>.Success(new UtcDateTime(value))
            : Result<UtcDateTime>.Failure(DomainErrors.UtcRequired);

    internal static Result<UtcDateTime> FromStorage(DateTime value) =>
        Create(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
