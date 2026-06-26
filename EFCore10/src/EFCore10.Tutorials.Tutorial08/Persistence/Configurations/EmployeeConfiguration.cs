using EFCore10.Tutorials.Tutorial08.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial08.Persistence;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    /// <summary>
    /// Configures TPH inheritance for employees and the University-Employee relation.
    /// </summary>
    public void Configure(EntityTypeBuilder<Employee> employee)
    {
        // Tutorial08 keeps only one inheritance example: Employee as TPH.
        // Tutorial07 remains the place for full TPH/TPT/TPC comparison.
        // TPH makes subtype-only columns nullable, so check constraints restore
        // the invariants that a professor has DepartmentId and an admin has Role.
        employee.ToTable(Tables.Employees, table =>
            {
                table.HasCheckConstraint(
                    "CK_Employees_Professor_DepartmentId",
                    "\"EmployeeType\" <> 'Professor' OR \"DepartmentId\" IS NOT NULL");
                table.HasCheckConstraint(
                    "CK_Employees_Administrative_Role",
                    "\"EmployeeType\" <> 'AdministrativeEmployee' OR \"Role\" IS NOT NULL");
            })
            .HasTableMapping(TableMappingMetadata.Tph);
        employee.UseTphMappingStrategy()
            .HasDiscriminator<string>("EmployeeType")
            .HasValue<Professor>("Professor")
            .HasValue<AdministrativeEmployee>("AdministrativeEmployee");

        // The aggregate keeps value objects; converters keep the schema simple.
        employee.HasKey(value => value.Id);
        employee.Property(value => value.Id)
            .HasConversion(value => value.Value, value => EmployeeId.FromStorage(value))
            .ValueGeneratedNever();
        employee.Property(value => value.Name)
            .HasConversion(value => value.Value, value => PersonName.FromStorage(value))
            .HasMaxLength(PersonName.MaxLength)
            .IsRequired();
        employee.Property(value => value.Email)
            .HasConversion(value => value.Value, value => EmailAddress.FromStorage(value))
            .HasMaxLength(EmailAddress.MaxLength)
            .IsRequired();
        // The value object normalizes shape; the unique index protects the
        // persistence boundary from duplicate employee emails.
        employee.HasIndex(value => value.Email)
            .IsUnique()
            .HasDatabaseName("IX_Employees_Email");

        // UniversityId is a shadow FK. Employee behavior talks to University as
        // an object reference; the FK is only persistence infrastructure.
        employee.Property<UniversityId>(Columns.UniversityId)
            .HasConversion(value => value.Value, value => UniversityId.FromStorage(value))
            .IsRequired();
        employee.HasIndex(Columns.UniversityId)
            .HasDatabaseName("IX_Employees_UniversityId");

        // Dates and status also stay as domain value objects. The converter is
        // the only place that knows their storage shape.
        employee.Property(value => value.HiredAtUtc)
            .HasConversion(value => value.Value, value => UtcDateTime.FromStorage(value))
            .IsRequired();
        employee.Property(value => value.DismissedAtUtc)
            .HasConversion(
                value => value == null ? (DateTime?)null : value.Value,
                value => value.HasValue ? UtcDateTime.FromStorage(value.Value) : null);
        employee.Property(value => value.Status)
            .HasConversion(value => value.Value, value => EmployeeStatus.FromStorage(value))
            .HasMaxLength(20)
            .IsRequired();
        employee.HasIndex("EmployeeType", nameof(Employee.Status))
            .HasDatabaseName("IX_Employees_EmployeeType_Status");

        employee.HasOne(value => value.University)
            .WithMany(value => value.Employees)
            .HasForeignKey(Columns.UniversityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
