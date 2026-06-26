namespace EFCore10.Tutorials.Tutorial08.Domain;

internal static class DomainErrors
{
    public const string RequiredTextCode = "text.required";
    public const string TextLengthCode = "text.length";

    public static DomainError RequiredText(string label) => new(RequiredTextCode, $"{label} is required.");

    public static DomainError TextLength(string label, int minLength, int maxLength) =>
        new(TextLengthCode, $"{label} must have between {minLength} and {maxLength} characters.");

    public static readonly DomainError CampusIdInvalid = new("campus.id_invalid", "Campus ID must be positive.");
    public static readonly DomainError CourseIdInvalid = new("course.id_invalid", "Course ID cannot be empty.");
    public static readonly DomainError DepartmentIdInvalid = new("department.id_invalid", "Department ID cannot be empty.");
    public static readonly DomainError EmployeeIdInvalid = new("employee.id_invalid", "Employee ID cannot be empty.");
    public static readonly DomainError StudentIdInvalid = new("student.id_invalid", "Student ID cannot be empty.");
    public static readonly DomainError UniversityIdInvalid = new("university.id_invalid", "University ID cannot be empty.");
    public static readonly DomainError CampusNameDuplicated = new("university.campus_name_duplicated", "University campus names must be unique.");
    public static readonly DomainError DepartmentNameDuplicated = new("university.department_name_duplicated", "Department names must be unique inside the university.");
    public static readonly DomainError ProfessorDepartmentMismatch = new("university.professor_department_mismatch", "Professor department must belong to the same university.");
    public static readonly DomainError EmployeeUniversityMismatch = new("university.employee_university_mismatch", "Employee must belong to the university.");
    public static readonly DomainError EmployeeAlreadyDismissed = new("employee.already_dismissed", "Employee is already dismissed.");
    public static readonly DomainError EmployeeDismissalDateInvalid = new("employee.dismissal_date_invalid", "Dismissal date cannot be earlier than hiring date.");
    public static readonly DomainError EmployeeDismissalBlocked = new("employee.dismissal_blocked", "Professor cannot be dismissed while assigned to an active course.");
    public static readonly DomainError ProfessorDismissed = new("professor.dismissed", "Dismissed professors cannot be assigned to courses.");
    public static readonly DomainError CourseDepartmentMismatch = new("course.department_mismatch", "Course professor must belong to the course department.");
    public static readonly DomainError CourseProfessorAlreadyAssigned = new("course.professor_already_assigned", "Course already has a professor.");
    public static readonly DomainError StudentAlreadyRegistered = new("student.already_registered", "Student is already registered for this course in the semester.");
    public static readonly DomainError StudentCreditLimitExceeded = new("student.credit_limit_exceeded", "Student cannot register for more than 40 credit points in the same semester.");
    public static readonly DomainError EnrollmentCourseMismatch = new("enrollment.course_mismatch", "Enrollment must belong to this course.");
    public static readonly DomainError EnrollmentAlreadyAddedToCourse = new("enrollment.already_added_to_course", "Course already has this enrollment.");
    public static readonly DomainError EnrollmentFinalGradeAlreadyRecorded = new("enrollment.final_grade_already_recorded", "Final grade is already recorded.");
    public static readonly DomainError EmailInvalid = new("email.invalid", "Email is invalid.");
    public static readonly DomainError CreditPointsInvalid = new("course.credit_points_invalid", "Course credit points must be between 1 and 40.");
    public static readonly DomainError SemesterYearInvalid = new("semester.year_invalid", "Semester year is outside the supported range.");
    public static readonly DomainError SemesterTermInvalid = new("semester.term_invalid", "Semester term must be 1 or 2.");
    public static readonly DomainError SemesterRequired = new("semester.required", "Semester is required.");
    public static readonly DomainError UtcRequired = new("datetime.utc_required", "Domain timestamps must be UTC.");
    public static readonly DomainError GradeInvalid = new("grade.invalid", "Final grade must be between 0 and 10.");
    public static readonly DomainError EmployeeStatusInvalid = new("employee.status_invalid", "Employee status is invalid.");
}
