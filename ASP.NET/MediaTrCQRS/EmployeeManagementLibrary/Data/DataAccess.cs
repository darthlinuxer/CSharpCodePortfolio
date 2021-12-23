using System.Collections.Generic;
using System.Linq;

namespace EmployeeManagementLibrary.Models
{
    public class DataAccess: IDataAccess
    {
        private List<EmployeeModel> _employees = new();

        public DataAccess()
        {
            _employees.Add(new EmployeeModel(){Id = 1, FirstName = "Camilo", LastName = "Chaves"});
            _employees.Add(new EmployeeModel(){Id = 2, FirstName = "Anakin", LastName = "Skywalker"});
        }

        public List<EmployeeModel> GetEmployees() => _employees;

        public EmployeeModel AddEmployee(string firstName, string lastName)
        {
            EmployeeModel employee = new() {FirstName = firstName, LastName = lastName};
            employee.Id = _employees.Max(x => x.Id) + 1;
            return employee;
        }
    }
}