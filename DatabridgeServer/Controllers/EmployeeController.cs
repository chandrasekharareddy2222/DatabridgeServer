using Microsoft.AspNetCore.Mvc;
using DatabridgeServer.Models;
using DatabridgeServer.Services.Employees;
using OfficeOpenXml;
using CsvHelper;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.Data.SqlClient;

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
            return Ok(new
            {
                message = message
            });
        }
        
        [HttpPut("update-employee/{empId}")]
        public async Task<IActionResult> UpdateEmployee(int empId, [FromBody] Employee request)
        {
            var message = await _employeeService.UpdateEmployeeAsync(empId, request.EmpName, request.DeptName);

            if (message == "Employee not found")
            {
                return NotFound(new { message });
            }

            return Ok(new
            {
                message = message
            });
        }

        [HttpDelete("delete-employee/{empId}")]
        public async Task<IActionResult> DeleteEmployee(int empId)
        {
            var message = await _employeeService.DeleteEmployeeAsync(empId);

            return Ok(new { message });
        }


        [HttpPost("bulk-import")]
        public async Task<IActionResult> BulkImport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
            {
                return BadRequest("Invalid file format. Please upload an Excel file (.xlsx).");
            }

            try
            {
                var result = await _employeeService.BulkImportEmployeesAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        

        [HttpDelete("delete-multiple")]
        public async Task<IActionResult> DeleteMultipleEmployees(
        [FromBody] DeleteMultipleEmployeesRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var message = await _employeeService
                .DeleteMultipleEmployeesAsync(request.EmpIds);

            return Ok(new { Message = message });

            
        }
    }
}