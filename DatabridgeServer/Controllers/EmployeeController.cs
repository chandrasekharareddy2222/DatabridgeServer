using DatabridgeServer.Models;
using DatabridgeServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabridgeServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // ================= GET ALL =================
        [HttpGet("get-all-full")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var result = await _employeeService.GetAllEmployeesFullAsync();
            return Ok(result);
        }

        // ================= ADD =================
        //[HttpPost("add")]

        [HttpPost("add")]
        public async Task<IActionResult> AddEmployee([FromBody] AddEmployeeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Now returns MessageResponse
            var result = await _employeeService.AddEmployeeAsync(request);

            return Ok(result);
        }

        // ================= GET BY ID =================
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            // The red underline will disappear now that the Interface is updated
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            return Ok(employee);
        }


        // ================= UPDATE NAME =================

        [HttpPut("update-name/{empId}")]
        public async Task<IActionResult> UpdateEmployeeName(
    int empId,
    [FromBody] UpdateEmployeeRequest request)
        {
            var message = await _employeeService.UpdateEmployeeNameAsync(empId, request.EmpName);

            return Ok(new { message });
        }


        // ================= DELETE =================
        [HttpDelete("{empId}")]
        public async Task<IActionResult> DeleteEmployee(int empId)
        {
            var result = await _employeeService.DeleteEmployeeAsync(empId);
            return Ok(result);
        }





    }
}
