namespace EFCore10.Tutorials.Tutorial08.Application.ReadModels;

internal interface IUniversityDashboardReadService
{
    string GetUniversityDashboardSql();

    Task<UniversityDashboardDto> GetUniversityDashboardAsync(CancellationToken cancellationToken);
}

internal interface ICourseAnalyticsReadService
{
    string GetCourseAnalyticsSql();

    Task<IReadOnlyList<CourseAnalyticsDto>> GetCourseAnalyticsAsync(CancellationToken cancellationToken);
}

internal interface IStudentProgressReadService
{
    string GetStudentProgressSql(StudentProgressQuery query);

    Task<IReadOnlyList<StudentProgressDto>> GetStudentProgressAsync(
        StudentProgressQuery query,
        CancellationToken cancellationToken);
}

internal interface IFacultyWorkloadReadService
{
    string GetFacultyWorkloadSql();

    Task<IReadOnlyList<FacultyWorkloadDto>> GetFacultyWorkloadAsync(CancellationToken cancellationToken);
}

internal sealed record StudentProgressQuery(int Year, int Term);

internal sealed record UniversityDashboardDto(
    string UniversityName,
    int CampusCount,
    int DepartmentCount,
    int EmployeeCount,
    int ProfessorCount,
    int AdministrativeEmployeeCount,
    int ActiveEmployeeCount,
    int CourseCount,
    int StudentCount,
    int EnrollmentCount);

internal sealed record CourseAnalyticsDto(
    string CourseCode,
    string CourseTitle,
    int CreditPoints,
    string SyllabusSummary,
    string? ProfessorName,
    string? DepartmentName,
    int EnrollmentCount,
    int GradedEnrollmentCount,
    decimal? AverageFinalGrade);

internal sealed record StudentProgressDto(
    string StudentName,
    string Semester,
    int TotalCreditPoints,
    int RemainingCreditPoints,
    decimal? AverageFinalGrade,
    IReadOnlyList<StudentCourseProgressDto> Courses);

internal sealed record StudentCourseProgressDto(
    string CourseCode,
    string CourseTitle,
    int CreditPoints,
    decimal? FinalGrade);

internal sealed record FacultyWorkloadDto(
    string ProfessorName,
    string DepartmentName,
    string Status,
    int CourseCount,
    int TotalCreditPoints,
    int EnrollmentCount,
    IReadOnlyList<FacultyCourseWorkloadDto> Courses);

internal sealed record FacultyCourseWorkloadDto(
    string CourseCode,
    string CourseTitle,
    int CreditPoints,
    int EnrollmentCount);
