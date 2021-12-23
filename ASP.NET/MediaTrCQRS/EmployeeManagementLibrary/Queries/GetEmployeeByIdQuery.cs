using EmployeeManagementLibrary.Models;
using MediatR;

namespace EmployeeManagementLibrary.Queries
{
    public record GetEmployeeByIdQuery(int id) : IRequest<EmployeeModel>;
}