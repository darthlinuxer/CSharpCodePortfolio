namespace EFCore10.Tutorials.Tutorial08.Domain;

internal static class DomainErrors
{
    public const string RequiredText = "text.required";
    public const string TextLength = "text.length";
    public const string CampusIdInvalid = "campus.id_invalid";
    public const string CourseIdInvalid = "course.id_invalid";
    public const string DepartmentIdInvalid = "department.id_invalid";
    public const string EmployeeIdInvalid = "employee.id_invalid";
    public const string StudentIdInvalid = "student.id_invalid";
    public const string UniversityIdInvalid = "university.id_invalid";
    public const string CampusNameDuplicated = "university.campus_name_duplicated";
    public const string DepartmentNameDuplicated = "university.department_name_duplicated";
    public const string ProfessorDepartmentMismatch = "university.professor_department_mismatch";
    public const string EmployeeUniversityMismatch = "university.employee_university_mismatch";
    public const string EmployeeAlreadyDismissed = "employee.already_dismissed";
    public const string EmployeeDismissalDateInvalid = "employee.dismissal_date_invalid";
    public const string EmployeeDismissalBlocked = "employee.dismissal_blocked";
    public const string ProfessorDismissed = "professor.dismissed";
    public const string CourseDepartmentMismatch = "course.department_mismatch";
    public const string CourseProfessorAlreadyAssigned = "course.professor_already_assigned";
    public const string StudentAlreadyRegistered = "student.already_registered";
    public const string StudentCreditLimitExceeded = "student.credit_limit_exceeded";
    public const string EnrollmentCourseMismatch = "enrollment.course_mismatch";
    public const string EnrollmentAlreadyAddedToCourse = "enrollment.already_added_to_course";
    public const string EnrollmentFinalGradeAlreadyRecorded = "enrollment.final_grade_already_recorded";
    public const string EmailInvalid = "email.invalid";
    public const string CreditPointsInvalid = "course.credit_points_invalid";
    public const string SemesterYearInvalid = "semester.year_invalid";
    public const string SemesterTermInvalid = "semester.term_invalid";
    public const string SemesterRequired = "semester.required";
    public const string UtcRequired = "datetime.utc_required";
    public const string GradeInvalid = "grade.invalid";
    public const string EmployeeStatusInvalid = "employee.status_invalid";
}
