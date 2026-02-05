using System.Data;
using Microsoft.Data.SqlClient;
using DatabridgeServer.Models;
using Microsoft.Extensions.Configuration;

namespace DatabridgeServer.Services.Students
{
    public class StudentService : IStudentService
    {
        private readonly IConfiguration _configuration;

        public StudentService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST
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

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        // GET ALL
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
                    StudentName = reader.GetString(0),
                    Age = reader.GetInt32(1),
                    DeptName = reader.GetString(2)
                });
            }

            return students;
        }

        // GET BY ID
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
                    StudentName = reader.GetString(0),
                    Age = reader.GetInt32(1),
                    DeptName = reader.GetString(2)
                };
            }

            return student;
        }

        // PUT
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

        // DELETE ✅ FIXED
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
    }
}
