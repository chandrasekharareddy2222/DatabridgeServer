using DatabridgeServer.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabridgeServer.Services.Members
{
    public class MemberService : IMemberService
    {
        private readonly string _connectionString;

        public MemberService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<string> InsertMemberAndBookAsync(MemberBookDto dto)
        {
            string resultMessage = "";

            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("InsertMemberandBooks", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Bookname", dto.Bookname);
                cmd.Parameters.AddWithValue("@MemberName", dto.MemberName);
                cmd.Parameters.AddWithValue("@MemberAge", dto.MemberAge);

                await conn.OpenAsync();

                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    resultMessage = reader["Message"].ToString();
                }
            }
            catch (SqlException ex)
            {
                resultMessage = "Database error: " + ex.Message;
            }
            catch (Exception ex)
            {
                resultMessage = "Unexpected error: " + ex.Message;
            }

            return resultMessage;
        }
    }
}
