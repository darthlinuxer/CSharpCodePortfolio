using AutoFac_Test.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoFac_Test.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public IActionResult GetEmployeeNames()
        {
            var employeeNames = _employeeService.GetEmployeeNames();

            return Ok(employeeNames);
        }
    }
}