namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record UniversityId
{
    private UniversityId(Guid value) => Value = value;

    public Guid Value { get; }

    internal static UniversityId New() => new(Guid.CreateVersion7());

    internal static Result<UniversityId> Create(Guid value) => FromStorage(value);

    internal static Result<UniversityId> FromStorage(Guid value) =>
        value == Guid.Empty
            ? Result<UniversityId>.Failure(DomainErrors.UniversityIdInvalid)
            : Result<UniversityId>.Success(new UniversityId(value));
}
