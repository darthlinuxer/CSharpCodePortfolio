using EFCore10.Tutorials.Tutorial08.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial08.Persistence;

internal sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    /// <summary>
    /// Configures the Student-Course join entity with enrollment payload.
    /// </summary>
    public void Configure(EntityTypeBuilder<Enrollment> enrollment)
    {
        // Enrollment is a join entity with payload, not a hidden many-to-many.
        // The IDs are explicit because they form the key and are part of this
        // relationship's own identity: student + course + semester.
        enrollment.ToTable(Tables.Enrollments)
            .HasTableMapping(TableMappingMetadata.JoinEntity);
        enrollment.HasKey(value => new { value.StudentId, value.CourseId, value.Semester });

        // These FKs are deliberately not shadow properties. The domain rule for
        // duplicate enrollment depends on CourseId plus Semester, so exposing
        // them here is useful and not just ORM bookkeeping.
        enrollment.Property(value => value.StudentId)
            .HasConversion(value => value.Value, value => StudentId.FromStorage(value).RequireValue())
            .IsRequired();
        enrollment.Property(value => value.CourseId)
            .HasConversion(value => value.Value, value => CourseId.FromStorage(value).RequireValue())
            .IsRequired();
        enrollment.Property(value => value.Semester)
            .HasConversion(value => value.Value, value => Semester.FromStorage(value).RequireValue())
            .HasMaxLength(8)
            .IsRequired();
        enrollment.HasIndex(value => new { value.Semester, value.CourseId })
            .HasDatabaseName("IX_Enrollments_Semester_CourseId");

        // Payload columns belong to the enrollment itself.
        enrollment.Property(value => value.EnrolledAtUtc)
            .HasConversion(value => value.Value, value => UtcDateTime.FromStorage(value).RequireValue())
            .IsRequired();
        enrollment.Property(value => value.FinalGrade)
            .HasConversion(
                value => value == null ? (decimal?)null : value.Value,
                value => value.HasValue ? Grade.FromStorage(value.Value).RequireValue() : null)
            .HasPrecision(4, 2);

        // Cascade is acceptable here: enrollment rows do not make sense without
        // either side of the relationship.
        enrollment.HasOne(value => value.Student)
            .WithMany(value => value.Enrollments)
            .HasForeignKey(value => value.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        enrollment.HasOne(value => value.Course)
            .WithMany(value => value.Enrollments)
            .HasForeignKey(value => value.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
