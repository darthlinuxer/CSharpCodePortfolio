using EFCore10.Tutorials.Tutorial08.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial08.Persistence;

internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    /// <summary>
    /// Configures student identity as an aggregate root.
    /// </summary>
    public void Configure(EntityTypeBuilder<Student> student)
    {
        // Student is an aggregate root, so it gets a concrete table and its own
        // identity. Course registration happens through Enrollment.
        student.ToTable(Tables.Students)
            .HasTableMapping(TableMappingMetadata.ConcreteTable);
        student.Navigation(value => value.Enrollments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Courses is a convenience projection over Enrollments. Persisting it
        // directly would duplicate the join mapping.
        student.Ignore(value => value.Courses);

        // Value objects keep validation in the domain; converters keep EF's
        // schema as ordinary scalar columns.
        student.HasKey(value => value.Id);
        student.Property(value => value.Id)
            .HasConversion(value => value.Value, value => new StudentId(value))
            .ValueGeneratedNever();
        student.Property(value => value.Name)
            .HasConversion(value => value.Value, value => PersonName.Create(value))
            .HasMaxLength(PersonName.MaxLength)
            .IsRequired();
        student.Property(value => value.Email)
            .HasConversion(value => value.Value, value => EmailAddress.Create(value))
            .HasMaxLength(EmailAddress.MaxLength)
            .IsRequired();
    }
}
