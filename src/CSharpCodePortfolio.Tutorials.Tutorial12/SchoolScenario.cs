using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial12;

internal sealed class SchoolScenario(DbContextOptions<SchoolDbContext> options)
{
    public async Task<SchoolReport> RunAsync(CancellationToken cancellationToken)
    {
        await using (var context = new SchoolDbContext(options))
        {
            await context.Database.EnsureDeletedAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var camilo = new Student { Name = "Camilo", Email = "camilo@example.com" };
            var aline = new Student { Name = "Aline", Email = "aline@example.com" };
            var teacher = new Teacher { Name = "Ozzy", Email = "ozzy@example.com" };
            var course = new Course { Name = "How to be a Rock Star", Teacher = teacher };
            course.Students.Add(camilo);
            course.Students.Add(aline);

            await context.Students.AddRangeAsync([camilo, aline], cancellationToken);
            await context.Teachers.AddAsync(teacher, cancellationToken);
            await context.Courses.AddAsync(course, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        await using var readContext = new SchoolDbContext(options);
        var students = await readContext.Students
            .AsNoTracking()
            .OrderBy(static student => student.Id)
            .Select(static student => $"#{student.Id} {student.Name} <{student.Email}>")
            .ToArrayAsync(cancellationToken);

        var teachers = await readContext.Teachers
            .AsNoTracking()
            .OrderBy(static teacher => teacher.Id)
            .Select(static teacher => $"#{teacher.Id} {teacher.Name} <{teacher.Email}>")
            .ToArrayAsync(cancellationToken);

        var savedCourse = await readContext.Courses
            .AsNoTracking()
            .Include(course => course.Teacher)
            .Include(course => course.Students)
            .SingleAsync(cancellationToken);

        var enrolledStudents = savedCourse.Students
            .OrderBy(static student => student.Id)
            .Select(static student => student.Name)
            .ToArray();

        var studentCount = await readContext.Students.CountAsync(cancellationToken);

        return new SchoolReport(
            students,
            teachers,
            $"#{savedCourse.Id} {savedCourse.Name} com {savedCourse.Teacher?.Name}",
            enrolledStudents,
            studentCount);
    }
}
