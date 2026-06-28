using EFCore10.Tutorials.Tutorial08.Application.Commands;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial08.Infrastructure.Persistence;

internal sealed class UniversitySampleStore(UniversityContext context) : IUniversitySampleStore
{
    public async Task RecreateAsync(
        University university,
        IReadOnlyCollection<Course> courses,
        IReadOnlyCollection<Student> students,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(university);
        ArgumentNullException.ThrowIfNull(courses);
        ArgumentNullException.ThrowIfNull(students);

        await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        context.Add(university);
        context.AddRange(courses);
        context.AddRange(students);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();
    }
}
