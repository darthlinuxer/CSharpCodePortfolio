using EFCore10.Tutorials.Tutorial08.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial08.Infrastructure.Persistence;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    /// <summary>
    /// Configures department table, university ownership and professor collection.
    /// </summary>
    public void Configure(EntityTypeBuilder<Department> department)
    {
        // Department is an internal entity of University: it has identity and a
        // table, but the tutorial does not expose it as an aggregate root.
        department.ToTable(Tables.Departments)
            .HasTableMapping(TableMappingMetadata.InternalEntity);
        department.Navigation(value => value.Professors)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // DepartmentId and DepartmentName stay in the domain as value objects.
        department.HasKey(value => value.Id);
        department.Property(value => value.Id)
            .HasConversion(value => value.Value, value => DepartmentId.FromStorage(value).RequireValue())
            .ValueGeneratedNever();
        department.Property(value => value.Name)
            .HasConversion(value => value.Value, value => DepartmentName.FromStorage(value).RequireValue())
            .HasMaxLength(DepartmentName.MaxLength)
            .IsRequired();

        // UniversityId is a shadow FK because Department already has the
        // University navigation. Keeping both in the domain would duplicate the
        // same association and make invariants easier to desynchronize.
        department.Property<UniversityId>(Columns.UniversityId)
            .HasConversion(value => value.Value, value => UniversityId.FromStorage(value).RequireValue())
            .IsRequired();
        department.HasIndex(Columns.UniversityId, nameof(Department.Name))
            .IsUnique()
            .HasDatabaseName("IX_Departments_UniversityId_Name");

        department.HasOne(value => value.University)
            .WithMany(value => value.Departments)
            .HasForeignKey(Columns.UniversityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
