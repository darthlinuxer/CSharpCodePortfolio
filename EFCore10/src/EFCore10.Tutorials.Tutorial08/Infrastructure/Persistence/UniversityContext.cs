using EFCore10.Tutorials.Tutorial08.Domain;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial08.Infrastructure.Persistence;

internal sealed class UniversityContext(DbContextOptions<UniversityContext> options) : DbContext(options)
{
    public DbSet<University> Universities => Set<University>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Professor> Professors => Set<Professor>();

    public DbSet<AdministrativeEmployee> AdministrativeEmployees => Set<AdministrativeEmployee>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UniversityContext).Assembly);
    }
}
