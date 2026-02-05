using DatabridgeServer.Models;
using DatabridgeServer.Services;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("add")]
        public async Task<IActionResult> AddEmployee([FromBody] AddEmployeeRequest request)
        {
            var response = await _employeeService.AddEmployeeAsync(request);

            if (response.Message == "Employee already exists")
            {
                return Conflict(response);
            }

            if (response.Message == "Employee inserted successfully")
            {
                return CreatedAtAction(nameof(AddEmployee), new { id = response.EmpId }, response);
            }

            if (response.Message?.Contains("required") == true ||
                response.Message?.Contains("invalid") == true)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
