namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed class UniversityCampus
{
    private UniversityCampus()
    {
    }

    internal UniversityCampus(CampusId id, CampusName name, CityName city)
    {
        Id = id;
        Name = name;
        City = city;
    }

    public CampusId Id { get; private set; } = null!;

    public CampusName Name { get; private set; } = null!;

    public CityName City { get; private set; } = null!;
}
