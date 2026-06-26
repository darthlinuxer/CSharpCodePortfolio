namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record CampusId
{
    private CampusId(int value) => Value = value;

    public int Value { get; }

    internal static CampusId Create(int value) =>
        value > 0 ? new CampusId(value) : throw new DomainException(DomainErrors.CampusIdInvalid, "Campus ID must be positive.");

    internal static CampusId From(int value) => Create(value);

    internal static CampusId FromStorage(int value) => Create(value);
}
