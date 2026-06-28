namespace EFCore10.Tutorials.Tutorial08.Domain.Institutional;

internal sealed record CampusId
{
    private CampusId(int value) => Value = value;

    public int Value { get; }

    internal static Result<CampusId> Create(int value) =>
        value > 0 ? Result<CampusId>.Success(new CampusId(value)) : Result<CampusId>.Failure(DomainErrors.CampusIdInvalid);

    internal static Result<CampusId> From(int value) => Create(value);

    internal static Result<CampusId> FromStorage(int value) => Create(value);
}
