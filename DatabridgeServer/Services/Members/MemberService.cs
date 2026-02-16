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

            await using var conn = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("GetAllMembers", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                members.Add(new MemberBookDto
                {
                    MemberName = reader.GetString(1),
                    MemberAge = reader.GetInt32(2),
                    Bookname = reader.GetString(3)
                });
            }

            return members;
        }


        //GET BY ID
        public async Task<MemberBookDto?> GetMemberByIdAsync(int memberId)
        {
            await using var conn = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("GetMemberById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

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
            await using var conn = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("UpdateMember", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MemberId", id);
            cmd.Parameters.AddWithValue("@MemberName", dto.MemberName);
            cmd.Parameters.AddWithValue("@MemberAge", dto.MemberAge);
            cmd.Parameters.AddWithValue("@Bookname", dto.Bookname);

            await conn.OpenAsync();
            var rows = (int)await cmd.ExecuteScalarAsync();

            return rows == 0 ? "NOT FOUND" : "SUCCESS";
        }


        //DELETE BY MEMBER
        public async Task<string> DeleteMemberAsync(int memberId)
        {
            await using var conn = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("DeleteMember", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MemberId", memberId);

            await conn.OpenAsync();
            var rows = (int)await cmd.ExecuteScalarAsync();

            return rows == 0 ? "NOT FOUND" : "SUCCESS";
        }

        //DELETE MULTIPLE
        public async Task<int> DeleteMembersAsync(List<int> memberIds)
        {
            var ids = string.Join(",", memberIds);

            await using var conn = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("DeleteMembers", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Ids", ids);

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync();
        }
    }




    
}
