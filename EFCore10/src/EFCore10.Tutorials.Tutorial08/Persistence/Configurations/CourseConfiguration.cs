using EFCore10.Tutorials.Tutorial08.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial08.Persistence;

internal sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    /// <summary>
    /// Configures course identity, owned syllabus and professor relation.
    /// </summary>
    public void Configure(EntityTypeBuilder<Course> course)
    {
        // Course is an aggregate root, so it receives its own concrete table.
        // It is not part of the Employee TPH hierarchy.
        course.ToTable(Tables.Courses)
            .HasTableMapping(TableMappingMetadata.ConcreteTable);
        course.Navigation(value => value.Enrollments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Students is a read-only projection over Enrollments. Mapping it as a
        // collection navigation would create a second relationship for the same
        // concept, so EF should ignore it.
        course.Ignore(value => value.Students);

        // IDs and scalar fields are value objects in the domain. EF stores them
        // as SQLite-friendly scalar columns through converters.
        course.HasKey(value => value.Id);
        course.Property(value => value.Id)
            .HasConversion(value => value.Value, value => CourseId.FromStorage(value))
            .ValueGeneratedNever();
        course.Property(value => value.Title)
            .HasConversion(value => value.Value, value => CourseTitle.FromStorage(value))
            .HasMaxLength(CourseTitle.MaxLength)
            .IsRequired();
        course.Property(value => value.Code)
            .HasConversion(value => value.Value, value => CourseCode.FromStorage(value))
            .HasMaxLength(CourseCode.MaxLength)
            .IsRequired();
        course.HasIndex(value => value.Code)
            .IsUnique()
            .HasDatabaseName("IX_Courses_Code");
        course.Property(value => value.CreditPoints)
            .HasConversion(value => value.Value, value => CreditPoints.FromStorage(value))
            .IsRequired();

        // DepartmentId is required because a course belongs to an academic
        // department even before it has a professor.
        course.Property<DepartmentId>(Columns.DepartmentId)
            .HasConversion(value => value.Value, value => DepartmentId.FromStorage(value))
            .HasColumnName(Columns.DepartmentId)
            .IsRequired();
        course.HasIndex(Columns.DepartmentId)
            .HasDatabaseName("IX_Courses_DepartmentId");

        // ProfessorId is intentionally a shadow FK. The domain needs the
        // Professor navigation to enforce behavior, not a duplicated ID
        // property beside it. The database still gets a ProfessorId column.
        course.Property<EmployeeId?>(Columns.ProfessorId)
            .HasConversion(
                value => value == null ? (Guid?)null : value.Value,
                value => value.HasValue ? EmployeeId.FromStorage(value.Value) : null)
            .HasColumnName(Columns.ProfessorId);
        course.HasIndex(Columns.ProfessorId)
            .HasDatabaseName("IX_Courses_ProfessorId");

        // Syllabus has no identity outside Course, so OwnsOne keeps it inside
        // the aggregate and stores its value-object columns in Courses.
        course.OwnsOne(value => value.Syllabus, syllabus =>
        {
            syllabus.HasTableMapping(TableMappingMetadata.OwnsOne);
            syllabus.Property(value => value.Summary)
                .HasConversion(value => value.Value, value => SyllabusSummary.FromStorage(value))
                .HasColumnName("SyllabusSummary")
                .HasMaxLength(SyllabusSummary.MaxLength)
                .IsRequired();
            syllabus.Property(value => value.Outcomes)
                .HasConversion(value => value.Value, value => SyllabusOutcomes.FromStorage(value))
                .HasColumnName("SyllabusOutcomes")
                .HasMaxLength(SyllabusOutcomes.MaxLength)
                .IsRequired();
        });

        // Department is mandatory in the domain because professor assignment
        // must compare Course.Department with Professor.Department.
        course.HasOne(value => value.Department)
            .WithMany()
            .HasForeignKey(Columns.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // The relationship is optional because hiring a professor and assigning
        // a course are separate domain actions. Course.AssignProfessor still
        // protects the active-professor and same-department invariants.
        course.HasOne(value => value.Professor)
            .WithMany()
            .HasForeignKey(Columns.ProfessorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
