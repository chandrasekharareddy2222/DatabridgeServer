using DatabridgeServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
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

        // ---------------- GET ALL MEMBERS ----------------
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
                    Memberid = reader.GetInt32(0),
                    MemberName = reader.GetString(1),
                    MemberAge = reader.GetInt32(2),
                    Bookname = reader.GetString(3)
                });
            }

            return members;
        }

        // ---------------- GET MEMBER BY ID ----------------
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
                Memberid = reader.GetInt32(0),
                MemberName = reader.GetString(1),
                MemberAge = reader.GetInt32(2),
                Bookname = reader.GetString(3)
            };
        }

        // ---------------- INSERT MEMBER & BOOK ----------------
        public async Task<string> InsertMemberAndBookAsync(MemberBookDto dto)
        {
            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("InsertMemberandBooks", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@BookName", dto.Bookname);
                cmd.Parameters.AddWithValue("@MemberName", dto.MemberName);
                cmd.Parameters.AddWithValue("@MemberAge", dto.MemberAge);

                await conn.OpenAsync();

                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return reader["Message"]?.ToString() ?? "No response";
                }

                return "No response from database";
            }
            catch (SqlException ex)
            {
                return "Database error: " + ex.Message;
            }
            catch (Exception ex)
            {
                return "Unexpected error: " + ex.Message;
            }
        }

        // ---------------- UPDATE MEMBER ----------------
        public async Task<string> UpdateMemberAsync(int memberId, MemberBookDto memberBookDto)
        {
            await using var conn = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("UpdateMember", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MemberId", memberId);
            cmd.Parameters.AddWithValue("@MemberName", memberBookDto.MemberName);
            cmd.Parameters.AddWithValue("@MemberAge", memberBookDto.MemberAge);
            cmd.Parameters.AddWithValue("@BookName", memberBookDto.Bookname);

            await conn.OpenAsync();
            var rows = (int)await cmd.ExecuteScalarAsync();
            return rows == 0 ? "NOT FOUND" : "SUCCESS";
        }

        // ---------------- DELETE MEMBER ----------------
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

        // ---------------- DELETE MULTIPLE MEMBERS ----------------
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

        // ---------------- PROCESS UPLOAD FILE ----------------
        public async Task<List<string>> ProcessFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new List<string> { "File is empty or null." };

            var extension = Path.GetExtension(file.FileName).ToLower();

            return extension switch
            {
                ".xlsx" => await ProcessExcelAsync(file),
                ".csv" => await ProcessCsvAsync(file),
                _ => new List<string> { "Unsupported file type." }
            };
        }

        // ---------------- PROCESS EXCEL ----------------
        private async Task<List<string>> ProcessExcelAsync(IFormFile file)
        {
            var messages = new List<string>();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);

                if (!package.Workbook.Worksheets.Any())
                {
                    messages.Add("Excel file contains no worksheets.");
                    return messages;
                }

                var sheet = package.Workbook.Worksheets.First(); // safe access

                if (sheet.Dimension == null)
                {
                    messages.Add("Worksheet is empty.");
                    return messages;
                }

                int rows = sheet.Dimension.Rows;

                for (int row = 2; row <= rows; row++)
                {
                    try
                    {
                        var bookName = sheet.Cells[row, 1].Text?.Trim();
                        var memberName = sheet.Cells[row, 2].Text?.Trim();
                        var ageText = sheet.Cells[row, 3].Text?.Trim();

                        if (string.IsNullOrWhiteSpace(bookName) ||
                            string.IsNullOrWhiteSpace(memberName) ||
                            string.IsNullOrWhiteSpace(ageText))
                        {
                            messages.Add($"Row {row}: One or more required fields are empty.");
                            continue;
                        }

                        if (!int.TryParse(ageText, out int age))
                        {
                            messages.Add($"Row {row}: Invalid age '{ageText}'.");
                            continue;
                        }

                        var msg = await ExecuteStoredProcedure(bookName, memberName, age);
                        messages.Add($"Row {row}: {msg}");
                    }
                    catch (Exception exRow)
                    {
                        messages.Add($"Row {row}: Error - {exRow.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                messages.Add($"Error processing Excel file: {ex.Message}");
            }

            return messages;
        }

        // ---------------- PROCESS CSV ----------------
        private async Task<List<string>> ProcessCsvAsync(IFormFile file)
        {
            var messages = new List<string>();

            using var reader = new StreamReader(file.OpenReadStream());
            int row = 1;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                row++;

                if (row == 2) continue; // skip header

                var values = line.Split(',');

                if (values.Length < 3)
                {
                    messages.Add($"Row {row}: Missing columns.");
                    continue;
                }

                if (!int.TryParse(values[2], out int age))
                {
                    messages.Add($"Row {row}: Invalid age '{values[2]}'.");
                    continue;
                }

                var msg = await ExecuteStoredProcedure(values[0], values[1], age);
                messages.Add($"Row {row}: {msg}");
            }

            return messages;
        }

        // ---------------- EXECUTE STORED PROCEDURE ----------------
        private async Task<string> ExecuteStoredProcedure(string bookName, string memberName, int age)
        {
            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("dbo.InsertMemberandBooks", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@BookName", bookName);
                cmd.Parameters.AddWithValue("@MemberName", memberName);
                cmd.Parameters.AddWithValue("@MemberAge", age);

                await conn.OpenAsync();

                // ExecuteReader to capture "Message" returned from SP
                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                    return reader["Message"]?.ToString() ?? "No response";

                return "No response from database";
            }
            catch (Exception ex)
            {
                return $"Error executing SP: {ex.Message}";
            }
        }
    }
}
