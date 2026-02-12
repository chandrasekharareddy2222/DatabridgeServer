using Microsoft.AspNetCore.Mvc;
using DatabridgeServer.Models;
using DatabridgeServer.Services.Employees;

namespace DatabridgeServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        [HttpGet("get-employee/{empId}")]
        public async Task<IActionResult> GetEmployee(int empId)
        {
            var result = await _employeeService.GetEmployeeByIdAsync(empId);

            if (result.employee == null)
                return NotFound(new { message = result.message });

            return Ok(result.employee);
        }

        [HttpPost("add-employee")]
        public async Task<IActionResult> AddEmployee([FromBody] Employee request)
        {
            var message = await _employeeService
                .AddEmployeeAsync(request.EmpName, request.DeptName);

            return Ok(new { message });
        }

        [HttpPut("update-employee/{empId}")]
        public async Task<IActionResult> UpdateEmployee(
    int empId,
    [FromBody] Employee request)
        {
            var message = await _employeeService
                .UpdateEmployeeAsync(empId, request.EmpName, request.DeptName);

            return Ok(new { message });
        }


        [HttpDelete("delete-employee/{empId}")]
        public async Task<IActionResult> DeleteEmployee(int empId)
        {
            var message = await _employeeService.DeleteEmployeeAsync(empId);

            return Ok(new { message });
        }
    }
}