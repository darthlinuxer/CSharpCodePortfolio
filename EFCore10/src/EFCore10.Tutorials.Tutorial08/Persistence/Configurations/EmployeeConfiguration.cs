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
        employee.ToTable(Tables.Employees)
            .HasTableMapping(TableMappingMetadata.Tph);
        employee.UseTphMappingStrategy()
            .HasDiscriminator<string>("EmployeeType")
            .HasValue<Professor>("Professor")
            .HasValue<AdministrativeEmployee>("AdministrativeEmployee");

        // The aggregate keeps value objects; converters keep the schema simple.
        employee.HasKey(value => value.Id);
        employee.Property(value => value.Id)
            .HasConversion(value => value.Value, value => new EmployeeId(value))
            .ValueGeneratedNever();
        employee.Property(value => value.Name)
            .HasConversion(value => value.Value, value => PersonName.Create(value))
            .HasMaxLength(PersonName.MaxLength)
            .IsRequired();
        employee.Property(value => value.Email)
            .HasConversion(value => value.Value, value => EmailAddress.Create(value))
            .HasMaxLength(EmailAddress.MaxLength)
            .IsRequired();

        // UniversityId is a shadow FK. Employee behavior talks to University as
        // an object reference; the FK is only persistence infrastructure.
        employee.Property<UniversityId>(Columns.UniversityId)
            .HasConversion(value => value.Value, value => new UniversityId(value))
            .IsRequired();
        employee.HasIndex(Columns.UniversityId)
            .HasDatabaseName("IX_Employees_UniversityId");

        // Dates and status also stay as domain value objects. The converter is
        // the only place that knows their storage shape.
        employee.Property(value => value.HiredAtUtc)
            .HasConversion(value => value.Value, value => UtcDateTime.Create(DateTime.SpecifyKind(value, DateTimeKind.Utc)))
            .IsRequired();
        employee.Property(value => value.DismissedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.Value : (DateTime?)null,
                value => value.HasValue ? UtcDateTime.Create(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)) : null);
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
