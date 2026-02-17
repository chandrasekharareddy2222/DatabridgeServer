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

        // ==============================
        // UPLOAD EXCEL/CSV
        // ==============================
        [HttpPost("upload-excel")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadStudentExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            try
            {
                if (extension == ".xlsx")
                {
                    var count = await _studentService.UploadStudentsFromExcelAsync(file);
                    return Ok(new { message = "Excel uploaded successfully", recordsInserted = count });
                }
                else if (extension == ".csv")
                {
                    var count = await _studentService.UploadStudentsFromCsvAsync(file);
                    return Ok(new { message = "CSV uploaded successfully", recordsInserted = count });
                }
                else
                {
                    return BadRequest("Only .xlsx or .csv files are allowed");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==============================
        // CREATE STUDENT(POST)
        // ==============================
        [HttpPost]
        public async Task<IActionResult> PostStudent([FromBody] Student student)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!System.Text.RegularExpressions.Regex.IsMatch(student.StudentName, @"^[A-Za-z]+$"))
            {
                return BadRequest(new { message = "Student name must contain only alphabets." });
            }


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
                return StatusCode(500, new { message = "Internal server error. Please try again." });
            }
        }

        // ==============================
        // GET ALL STUDENTS
        // ==============================
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
                return StatusCode(500, new { message = "An unexpected error occurred while fetching students." });
            }
        }

        // ==============================
        // GET STUDENT BY ID
        // ==============================
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
                return StatusCode(500, new { message = "Internal server error. Please try again." });
            }
        }

        // ==============================
        // UPDATE STUDENT
        // ==============================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student student)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //  Added same validation as POST
            if (!System.Text.RegularExpressions.Regex.IsMatch(student.StudentName, @"^[A-Za-z]+$"))
            {
                return BadRequest(new { message = "Student name must contain only alphabets." });
            }

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
                return StatusCode(500, new { message = "Internal server error. Please try again." });
            }
        }


        // ==============================
        // DELETE STUDENT
        // ==============================
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
                return StatusCode(500, new { message = "Internal server error. Please try again." });
            }
        }

        // ==============================
        // BATCH DELETE STUDENTS
        // ==============================
        [HttpPost("delete-batch")]
        public async Task<IActionResult> DeleteStudentsBatch([FromBody] List<int> studentIds)
        {
            if (studentIds == null || studentIds.Count == 0)
                return BadRequest(new { message = "No Student IDs provided." });

            try
            {
                var (rowsDeleted, missingIds) = await _studentService.DeleteStudentsBatchAsync(studentIds);

                return Ok(new
                {
                    DeletedRows = rowsDeleted,
                    MissingIds = missingIds
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }
    }
}
