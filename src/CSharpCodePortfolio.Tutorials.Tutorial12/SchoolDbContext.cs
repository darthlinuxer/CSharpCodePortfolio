using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial12;

internal sealed class SchoolDbContext(DbContextOptions<SchoolDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();

    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(student =>
        {
            student.HasKey(entity => entity.Id);
            student.Property(entity => entity.Name).IsRequired().HasMaxLength(80);
            student.Property(entity => entity.Email).IsRequired().HasMaxLength(120);
        });

        modelBuilder.Entity<Teacher>(teacher =>
        {
            teacher.HasKey(entity => entity.Id);
            teacher.Property(entity => entity.Name).IsRequired().HasMaxLength(80);
            teacher.Property(entity => entity.Email).IsRequired().HasMaxLength(120);
        });

        modelBuilder.Entity<Course>(course =>
        {
            course.HasKey(entity => entity.Id);
            course.Property(entity => entity.Name).IsRequired().HasMaxLength(120);
            course
                .HasOne(entity => entity.Teacher)
                .WithMany(teacher => teacher.Courses)
                .HasForeignKey(entity => entity.TeacherId)
                .IsRequired();
            course
                .HasMany(entity => entity.Students)
                .WithMany(student => student.Courses)
                .UsingEntity("TblCourseStudent");
        });
    }
}
