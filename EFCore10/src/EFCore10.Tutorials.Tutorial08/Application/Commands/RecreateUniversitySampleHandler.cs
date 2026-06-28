namespace EFCore10.Tutorials.Tutorial08.Application.Commands;

internal sealed record RecreateUniversitySampleCommand;

internal sealed record UniversitySampleDto(
    string UniversityName,
    int DepartmentCount,
    int CourseCount,
    int StudentCount,
    int EnrollmentCount);

internal interface IUniversitySampleStore
{
    Task RecreateAsync(
        University university,
        IReadOnlyCollection<Course> courses,
        IReadOnlyCollection<Student> students,
        CancellationToken cancellationToken);
}

internal sealed class RecreateUniversitySampleHandler(IUniversitySampleStore store)
{
    /// <summary>
    /// Recreates the database and saves the sample university through domain behavior.
    /// </summary>
    public async Task<Result<UniversitySampleDto>> HandleAsync(
        RecreateUniversitySampleCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sample = CreateUniversitySample();
        await store
            .RecreateAsync(sample.University, sample.Courses, sample.Students, cancellationToken)
            .ConfigureAwait(false);

        return Result<UniversitySampleDto>.Success(new UniversitySampleDto(
            sample.University.Name.Value,
            sample.University.Departments.Count,
            sample.Courses.Count,
            sample.Students.Count,
            sample.EnrollmentCount));
    }

    private static SampleGraph CreateUniversitySample()
    {
        var hiredAtUtc = DateTime.SpecifyKind(new DateTime(2026, 6, 25, 14, 0, 0), DateTimeKind.Utc);
        var enrolledAtUtc = hiredAtUtc.AddHours(2);

        var university = University.Create("Contoso University").RequireValue();
        university.AddCampus("Main Campus", "Sao Paulo").RequireSuccess();
        university.AddCampus("Research Campus", "Campinas").RequireSuccess();

        var computerScience = university.OpenDepartment("Computer Science").RequireValue();
        var dataScience = university.OpenDepartment("Data Science").RequireValue();
        var grace = university.HireProfessor(
            "Grace Hopper",
            "grace.hopper@contoso.edu",
            computerScience,
            hiredAtUtc).RequireValue();
        var katherine = university.HireProfessor(
            "Katherine Johnson",
            "katherine.johnson@contoso.edu",
            dataScience,
            hiredAtUtc).RequireValue();
        university.HireAdministrativeEmployee(
            "Alan Turing",
            "alan.turing@contoso.edu",
            "Registrar",
            hiredAtUtc).RequireValue();

        var relationshipMapping = Course.Create(
            computerScience.Id,
            "EF Core Relationship Mapping",
            "CS-EF-101",
            20,
            "Modelar relacoes universitarias com EF Core.",
            "Distinguir aggregate root, entidade interna, OwnsOne, OwnsMany, HasOne e HasMany.").RequireValue();
        relationshipMapping.AssignProfessor(grace.ToAssignmentSnapshot()).RequireSuccess();

        var domainModeling = Course.Create(
            computerScience.Id,
            "DDD Tactical Modeling",
            "CS-DDD-201",
            15,
            "Modelar invariantes e boundaries de aggregates.",
            "Aplicar value objects, entidades internas e regras no dominio.").RequireValue();
        domainModeling.AssignProfessor(grace.ToAssignmentSnapshot()).RequireSuccess();

        var queryOptimization = Course.Create(
            dataScience.Id,
            "EF Core Query Optimization",
            "CS-EF-202",
            10,
            "Projetar read models eficientes para relatorios academicos.",
            "Aplicar Select, AsNoTracking, agregacoes e indices em consultas EF Core.").RequireValue();
        queryOptimization.AssignProfessor(katherine.ToAssignmentSnapshot()).RequireSuccess();

        var reporting = Course.Create(
            dataScience.Id,
            "Campus Operations Reporting",
            "CS-OPS-105",
            5,
            "Transformar relacoes academicas em indicadores operacionais.",
            "Ler dados relacionais sem expor entidades de dominio como contrato.").RequireValue();

        var ana = Student.Create("Ana Pereira", "ana.pereira@contoso.edu").RequireValue();
        var bia = Student.Create("Bia Santos", "bia.santos@contoso.edu").RequireValue();
        var caio = Student.Create("Caio Lima", "caio.lima@contoso.edu").RequireValue();

        var anaCourses = new List<CourseEnrollmentSnapshot>();
        RegisterForCourse(ana, relationshipMapping, anaCourses, enrolledAtUtc, 9.5m);
        RegisterForCourse(ana, domainModeling, anaCourses, enrolledAtUtc, 8.8m);

        var biaCourses = new List<CourseEnrollmentSnapshot>();
        RegisterForCourse(bia, relationshipMapping, biaCourses, enrolledAtUtc, 8.0m);
        RegisterForCourse(bia, queryOptimization, biaCourses, enrolledAtUtc, 9.2m);
        RegisterForCourse(bia, reporting, biaCourses, enrolledAtUtc, 8.5m);

        var caioCourses = new List<CourseEnrollmentSnapshot>();
        RegisterForCourse(caio, domainModeling, caioCourses, enrolledAtUtc, 7.6m);
        RegisterForCourse(caio, queryOptimization, caioCourses, enrolledAtUtc, 9.0m);

        return new SampleGraph(
            university,
            [relationshipMapping, domainModeling, queryOptimization, reporting],
            [ana, bia, caio],
            ana.Enrollments.Count + bia.Enrollments.Count + caio.Enrollments.Count);
    }

    private static void RegisterForCourse(
        Student student,
        Course course,
        List<CourseEnrollmentSnapshot> currentSemesterCourses,
        DateTime enrolledAtUtc,
        decimal finalGrade)
    {
        var courseSnapshot = course.ToEnrollmentSnapshot();
        var enrollment = student.RegisterForCourse(courseSnapshot, currentSemesterCourses, 2026, 1, enrolledAtUtc).RequireValue();
        enrollment.RecordFinalGrade(finalGrade).RequireSuccess();
        currentSemesterCourses.Add(courseSnapshot);
    }

    private sealed record SampleGraph(
        University University,
        IReadOnlyList<Course> Courses,
        IReadOnlyList<Student> Students,
        int EnrollmentCount);
}
