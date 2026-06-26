namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed class Department : DomainEntity<DepartmentId>
{
    private readonly List<Professor> _professors = [];

    private Department()
    {
    }

    private Department(DepartmentName name, University university)
        : base(DepartmentId.New())
    {
        ArgumentNullException.ThrowIfNull(university);

        Name = name;
        University = university;
    }

    public DepartmentName Name { get; private set; } = null!;

    public University University { get; private set; } = null!;

    public IReadOnlyCollection<Professor> Professors => _professors;

    internal static Department Create(DepartmentName name, University university) => new(name, university);

    internal void AddProfessor(Professor professor)
    {
        ArgumentNullException.ThrowIfNull(professor);

        if (_professors.Any(value => value.Id == professor.Id))
            return;

        _professors.Add(professor);
    }
}
