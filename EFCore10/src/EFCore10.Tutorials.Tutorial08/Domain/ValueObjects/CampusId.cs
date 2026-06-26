namespace EFCore10.Tutorials.Tutorial08.Domain;

internal readonly record struct CampusId(int Value)
{
    internal static CampusId From(int value) =>
        value > 0 ? new CampusId(value) : throw new DomainException(DomainErrors.CampusIdInvalid, "Campus ID must be positive.");
}
