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

        // POST (Already working)
        [HttpPost]
        public async Task<IActionResult> PostStudent([FromBody] Student student)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _studentService.InsertStudentAsync(student);
                return Ok(new { message = "Student registered successfully" });
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

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            try
            {
                var students = await _studentService.GetAllStudentsAsync();
                return Ok(students);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Internal server error. Please try again."
                });
            }
        }

        // GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);

                if (student == null)
                    return NotFound(new { message = "Student not found" });

                return Ok(student);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Internal server error. Please try again."
                });
            }
        }

        // PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student student)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _studentService.UpdateStudentAsync(id, student);

                if (!updated)
                    return NotFound(new { message = "Student not found" });

                return Ok(new { message = "Student updated successfully" });
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

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                var deleted = await _studentService.DeleteStudentAsync(id);

                if (!deleted)
                    return NotFound(new { message = "Student not found" });

                return Ok(new { message = "Student deleted successfully" });
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
