using DatabridgeServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;

namespace DatabridgeServer.Services.Students
{
    public class StudentService : IStudentService
    {
        private readonly IConfiguration _configuration;

        public StudentService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
            
        // ============================
        // EXCEL UPLOAD (MAX 1000)
        // ============================
        public async Task<int> UploadStudentsFromExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("No file uploaded");

            

            int insertedCount = 0;

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null || worksheet.Dimension == null)
                throw new Exception("Excel sheet is empty");

            int totalRows = worksheet.Dimension.Rows - 1;
            if (totalRows > 1000)
                throw new Exception("Maximum 1000 records allowed per upload");

            for (int row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                try
                {
                    var name = worksheet.Cells[row, 1].Text?.Trim();
                    var ageText = worksheet.Cells[row, 2].Text;
                    var dept = worksheet.Cells[row, 3].Text?.Trim();

                    if (string.IsNullOrWhiteSpace(name) ||
                        string.IsNullOrWhiteSpace(dept) ||
                        !int.TryParse(ageText, out int age))
                        continue;

                    var student = new Student
                    {
                        StudentName = name,
                        Age = age,
                        DeptName = dept
                    };

                    await InsertStudentAsync(student);
                    insertedCount++;
                }
                catch (SqlException)
                {
                    // Skip duplicate or invalid records
                    continue;
                }
            }

            return insertedCount;
        }
        // ============================
        // CSV UPLOAD 
        // ============================
        
        public async Task<int> UploadStudentsFromCsvAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("No file uploaded");

            int insertedCount = 0;
            int rowCount = 0;

            using var reader = new StreamReader(file.OpenReadStream());
            string? line;
            bool isHeader = true;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (isHeader)
                {
                    isHeader = false;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                rowCount++;
                if (rowCount > 1000)
                    throw new Exception("Maximum 1000 records allowed per upload");

                try
                {
                    var values = line.Split(',');

                    if (values.Length < 3)
                        continue;

                    var name = values[0]?.Trim();
                    var ageText = values[1];
                    var dept = values[2]?.Trim();

                    if (string.IsNullOrWhiteSpace(name) ||
                        string.IsNullOrWhiteSpace(dept) ||
                        !int.TryParse(ageText, out int age))
                        continue;

                    var student = new Student
                    {
                        StudentName = name,
                        Age = age,
                        DeptName = dept
                    };

                    await InsertStudentAsync(student);
                    insertedCount++;
                }
                catch (SqlException)
                {
                    // Skip duplicate or invalid records
                    continue;
                }
            }

            return insertedCount;
        }

        // ============================
        // POST
        // ============================
        public async Task InsertStudentAsync(Student student)
        {
            using SqlConnection connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            using SqlCommand command = new SqlCommand("dbo.InsertStudentDetails", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentName", student.StudentName);
            command.Parameters.AddWithValue("@Age", student.Age);
            command.Parameters.AddWithValue("@DeptName", student.DeptName);

            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                // This will catch RAISERROR messages from SQL
                throw; // or handle it in a nicer way
            }
        }

        // ============================
        // GET ALL
        // ============================
        public async Task<List<Student>> GetAllStudentsAsync()
        {
            var students = new List<Student>();

            using SqlConnection connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            using SqlCommand command = new SqlCommand("dbo.GetAllStudents", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                students.Add(new Student
                {
                    StudentID = reader.GetInt32(reader.GetOrdinal("StudentID")),
                    StudentName = reader.GetString(reader.GetOrdinal("StudentName")),
                    Age = reader.GetInt32(reader.GetOrdinal("Age")),
                    DeptName = reader.GetString(reader.GetOrdinal("DeptName"))
                });
            }

            return students;
        }

        // ============================
        // GET BY ID
        // ============================
        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            Student? student = null;

            using SqlConnection connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            using SqlCommand command = new SqlCommand("dbo.GetStudentById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentId", id);

            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                student = new Student
                {
                    StudentID = reader.GetInt32(reader.GetOrdinal("StudentID")),
                    StudentName = reader.GetString(reader.GetOrdinal("StudentName")),
                    Age = reader.GetInt32(reader.GetOrdinal("Age")),
                    DeptName = reader.GetString(reader.GetOrdinal("DeptName"))
                };
            }

            return student;
        }

        // ============================
        // UPDATE
        // ============================
        public async Task<bool> UpdateStudentAsync(int id, Student student)
        {
            using SqlConnection connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            using SqlCommand command = new SqlCommand("dbo.UpdateStudent", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentId", id);
            command.Parameters.AddWithValue("@StudentName", student.StudentName);
            command.Parameters.AddWithValue("@Age", student.Age);
            command.Parameters.AddWithValue("@DeptName", student.DeptName);

            var rowsAffected = new SqlParameter("@RowsAffected", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(rowsAffected);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return (int)rowsAffected.Value > 0;
        }

        // ============================
        // DELETE
        // ============================
        public async Task<bool> DeleteStudentAsync(int id)
        {
            using SqlConnection connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            using SqlCommand command = new SqlCommand("dbo.DeleteStudent", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@StudentId", id);

            var rowsAffected = new SqlParameter("@RowsAffected", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(rowsAffected);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return (int)rowsAffected.Value > 0;

           


        }


        public async Task<(int RowsDeleted, List<int> MissingIds)> DeleteStudentsBatchAsync(List<int> studentIds)
        {
            if (studentIds == null || studentIds.Count == 0)
                return (0, new List<int>());

            // Create a DataTable for the TVP
            var dataTable = new DataTable();
            dataTable.Columns.Add("StudentID", typeof(int));

            foreach (var id in studentIds)
            {
                dataTable.Rows.Add(id);
            }

            var paramIds = new SqlParameter("@StudentIds", SqlDbType.Structured)
            {
                TypeName = "dbo.StudentIdTableType",
                Value = dataTable
            };

            var paramRowsAffected = new SqlParameter("@RowsAffected", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            var missingIds = new List<int>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            using (var command = new SqlCommand("dbo.DeleteStudentsBatchEnhanced", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(paramIds);
                command.Parameters.Add(paramRowsAffected);

                await connection.OpenAsync();

                // Read missing IDs if the SP returns them
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        missingIds.Add(reader.GetInt32(0));
                    }
                }

                await connection.CloseAsync();
            }

            int rowsDeleted = (int)paramRowsAffected.Value;

            return (rowsDeleted, missingIds);
        }

    }
}
