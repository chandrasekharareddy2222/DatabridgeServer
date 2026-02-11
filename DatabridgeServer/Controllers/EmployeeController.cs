using DatabridgeServer.Models;
using DatabridgeServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DatabridgeServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet("get-all-full")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var result = await _employeeService.GetAllEmployeesFullAsync();
            return Ok(result);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddEmployee([FromBody] AddEmployeeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _employeeService.AddEmployeeAsync(request);
            return Ok(result);
        }
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }
            return Ok(employee);
        }
        [HttpPut("update-name/{empId}")]
        public async Task<IActionResult> UpdateEmployeeName(
        int empId,
        [FromBody] UpdateEmployeeRequest request)
        {
            var message = await _employeeService.UpdateEmployeeNameAsync(empId, request.EmpName);
            return Ok(new { message });
        }
        [HttpDelete("{empId}")]
        public async Task<IActionResult> DeleteEmployee(int empId)
        {
            var result = await _employeeService.DeleteEmployeeAsync(empId);
            return Ok(result);
        }
    }
}
