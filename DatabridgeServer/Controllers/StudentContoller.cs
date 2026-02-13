using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using DatabridgeServer.Models;
using DatabridgeServer.Services.Students;

namespace DatabridgeServer.Controllers
{
    [ApiController]                     
    [Route("api/[controller]")]         
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPost]                       
        public async Task<IActionResult> PostStudent([FromBody] Student student)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _studentService.InsertStudentAsync(student);
                return Ok(new { message = "Student registered successfully " });
            }
            catch (SqlException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Internal server error. Please try again."
                });
            }
        }
    }
}
