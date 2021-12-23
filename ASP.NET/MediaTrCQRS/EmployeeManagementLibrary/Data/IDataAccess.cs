using System.Collections.Generic;

namespace EmployeeManagementLibrary.Models
{
    public interface IDataAccess
    {
        List<EmployeeModel> GetEmployees();
        EmployeeModel AddEmployee(string firstName, string lastName);
    }
}