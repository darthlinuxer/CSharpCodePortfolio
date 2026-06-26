using EFCore10.Tutorials.Tutorial08.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial08.Persistence;

internal sealed class ProfessorConfiguration : IEntityTypeConfiguration<Professor>
{
    /// <summary>
    /// Configures the Professor-Department relation inside the Employee TPH table.
    /// </summary>
    public void Configure(EntityTypeBuilder<Professor> professor)
    {
        // DepartmentId is a shadow FK. Professor behavior uses the Department
        // navigation; the FK column exists for the relational model only.
        professor.Property<DepartmentId>(Columns.DepartmentId)
            .HasConversion(value => value.Value, value => DepartmentId.FromStorage(value).RequireValue())
            .IsRequired();
        professor.HasIndex(Columns.DepartmentId)
            .HasDatabaseName("IX_Employees_DepartmentId");

        professor.HasOne(value => value.Department)
            .WithMany(value => value.Professors)
            .HasForeignKey(Columns.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
