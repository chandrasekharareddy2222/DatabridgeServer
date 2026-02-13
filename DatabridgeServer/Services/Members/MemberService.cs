using DatabridgeServer.Models;
using Microsoft.AspNetCore.Mvc;
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

        //GET
        public async Task<List<MemberBookDto>> GetAllMembersAsync()
        {
            var members = new List<MemberBookDto>();

            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand(@"
            SELECT 
                m.Memberid,
                m.MemberName,
                m.MemberAge,
                b.Bookname
            FROM Members m
            INNER JOIN Books b ON m.Bookid = b.Bookid
        ", conn);

                await conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    members.Add(new MemberBookDto
                    {
                      
                        Bookname = reader.GetString(3),
                        MemberName = reader.GetString(1),
                        MemberAge = reader.GetInt32(2)
                        
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching members", ex);
            }

            return members;
        }

        //GET BY ID
        public async Task<MemberBookDto?> GetMemberByIdAsync(int memberId)
        {
            await using var conn = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(@"
        SELECT 
            m.Memberid,
            m.MemberName,
            m.MemberAge,
            b.Bookname
        FROM Members m
        INNER JOIN Books b ON m.Bookid = b.Bookid
        WHERE m.Memberid = @MemberId
    ", conn);

            cmd.Parameters.AddWithValue("@MemberId", memberId);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new MemberBookDto
            {
               
                MemberName = reader.GetString(1),
                MemberAge = reader.GetInt32(2),
                Bookname = reader.GetString(3)
            };
        }



        //POST
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

        //PUT 
        public async Task<string> UpdateMemberAsync(int id, MemberBookDto dto)
        {
            
            if (dto == null)
                return "Invalid data";

            if (string.IsNullOrWhiteSpace(dto.MemberName) || dto.MemberAge <= 0 || string.IsNullOrWhiteSpace(dto.Bookname))
                return "Invalid member details";

            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand(@"
            UPDATE Members
            SET MemberName = @MemberName,
                MemberAge = @MemberAge,
                Bookid = (SELECT Bookid FROM Books WHERE Bookname = @Bookname)
            WHERE Memberid = @Memberid
        ", conn);

                cmd.Parameters.AddWithValue("@Memberid", id);
                cmd.Parameters.AddWithValue("@MemberName", dto.MemberName);
                cmd.Parameters.AddWithValue("@MemberAge", dto.MemberAge);
                cmd.Parameters.AddWithValue("@Bookname", dto.Bookname);

                await conn.OpenAsync();
                var rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                    return $"Member with ID {id} not found";

                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        //DELETE BY MEMBER
        public async Task<string> DeleteMemberAsync(int memberId)
        {
            await using var conn = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(@"
        DELETE FROM Members
        WHERE MemberId = @MemberId;

        SELECT 
            CASE WHEN @@ROWCOUNT = 0 THEN 'NOT FOUND'
                 ELSE 'SUCCESS'
            END AS Message;
    ", conn);

            cmd.Parameters.AddWithValue("@MemberId", memberId);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return reader["Message"].ToString();

            return "DELETE FAILED";
        }

        public async Task<int> DeleteMembersAsync(List<int> memberIds)
        {
            if (memberIds == null || memberIds.Count == 0)
                return 0;

            var ids = string.Join(",", memberIds);

            await using var conn = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand($@"
        DELETE FROM Members
        WHERE MemberId IN ({ids});
        SELECT @@ROWCOUNT;
    ", conn);

            await conn.OpenAsync();
            var deletedCount = (int)await cmd.ExecuteScalarAsync();

            return deletedCount;
        }




    }
}
