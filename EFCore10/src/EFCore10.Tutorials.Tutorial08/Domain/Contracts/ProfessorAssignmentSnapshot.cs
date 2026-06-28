namespace EFCore10.Tutorials.Tutorial08.Domain.Contracts;

internal sealed record ProfessorAssignmentSnapshot(EmployeeId ProfessorId, DepartmentId DepartmentId, EmployeeStatus Status);
