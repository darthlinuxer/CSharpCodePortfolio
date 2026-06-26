namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record UniversityId
{
    private UniversityId(Guid value) => Value = value;

    public Guid Value { get; }

    internal static UniversityId New() => FromStorage(Guid.CreateVersion7());

    internal static UniversityId Create(Guid value) => FromStorage(value);

    internal static UniversityId FromStorage(Guid value) =>
        value == Guid.Empty
            ? throw new DomainException(DomainErrors.UniversityIdInvalid, "University ID cannot be empty.")
            : new UniversityId(value);
}
