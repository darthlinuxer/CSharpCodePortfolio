using EFCore10.Tutorials.Tutorial08.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial08.Infrastructure.Persistence;

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
        // IDs and scalar fields are value objects in the domain. EF stores them
        // as SQLite-friendly scalar columns through converters.
        course.HasKey(value => value.Id);
        course.Property(value => value.Id)
            .HasConversion(value => value.Value, value => CourseId.FromStorage(value).RequireValue())
            .ValueGeneratedNever();
        course.Property(value => value.Title)
            .HasConversion(value => value.Value, value => CourseTitle.FromStorage(value).RequireValue())
            .HasMaxLength(CourseTitle.MaxLength)
            .IsRequired();
        course.Property(value => value.Code)
            .HasConversion(value => value.Value, value => CourseCode.FromStorage(value).RequireValue())
            .HasMaxLength(CourseCode.MaxLength)
            .IsRequired();
        course.HasIndex(value => value.Code)
            .IsUnique()
            .HasDatabaseName("IX_Courses_Code");
        course.Property(value => value.CreditPoints)
            .HasConversion(value => value.Value, value => CreditPoints.FromStorage(value).RequireValue())
            .IsRequired();

        // DepartmentId is required because a course belongs to an academic
        // department even before it has a professor.
        course.Property(value => value.DepartmentId)
            .HasConversion(value => value.Value, value => DepartmentId.FromStorage(value).RequireValue())
            .HasColumnName(Columns.DepartmentId)
            .IsRequired();
        course.HasIndex(Columns.DepartmentId)
            .HasDatabaseName("IX_Courses_DepartmentId");

        course.Property(value => value.ProfessorId)
            .HasConversion(
                value => value == null ? (Guid?)null : value.Value,
                value => value.HasValue ? EmployeeId.FromStorage(value.Value).RequireValue() : null)
            .HasColumnName(Columns.ProfessorId);
        course.HasIndex(Columns.ProfessorId)
            .HasDatabaseName("IX_Courses_ProfessorId");

        // Syllabus has no identity outside Course, so OwnsOne keeps it inside
        // the aggregate and stores its value-object columns in Courses.
        course.OwnsOne(value => value.Syllabus, syllabus =>
        {
            syllabus.HasTableMapping(TableMappingMetadata.OwnsOne);
            syllabus.Property(value => value.Summary)
                .HasConversion(value => value.Value, value => SyllabusSummary.FromStorage(value).RequireValue())
                .HasColumnName("SyllabusSummary")
                .HasMaxLength(SyllabusSummary.MaxLength)
                .IsRequired();
            syllabus.Property(value => value.Outcomes)
                .HasConversion(value => value.Value, value => SyllabusOutcomes.FromStorage(value).RequireValue())
                .HasColumnName("SyllabusOutcomes")
                .HasMaxLength(SyllabusOutcomes.MaxLength)
                .IsRequired();
        });

        // Cross-context relationships stay relational only. The CourseCatalog
        // domain stores IDs and receives snapshots instead of holding
        // Institutional aggregate references.
        course.HasOne<Department>()
            .WithMany()
            .HasForeignKey(value => value.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        course.HasOne<Professor>()
            .WithMany()
            .HasForeignKey(value => value.ProfessorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
