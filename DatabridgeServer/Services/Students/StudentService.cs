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
    }
}
