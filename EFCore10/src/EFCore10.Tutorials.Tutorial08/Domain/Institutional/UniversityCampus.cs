namespace EFCore10.Tutorials.Tutorial08.Domain.Institutional;

internal sealed class UniversityCampus
{
    private UniversityCampus()
    {
    }

    private UniversityCampus(CampusId id, CampusName name, CityName city)
    {
        Id = id;
        Name = name;
        City = city;
    }

    public CampusId Id { get; private set; } = null!;

    public CampusName Name { get; private set; } = null!;

    public CityName City { get; private set; } = null!;

    internal static UniversityCampus Create(CampusId id, CampusName name, CityName city) => new(id, name, city);
}
