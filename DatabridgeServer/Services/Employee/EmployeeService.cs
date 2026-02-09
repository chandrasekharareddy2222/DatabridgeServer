
using DatabridgeServer.Data;
using DatabridgeServer.Models;
using DatabridgeServer.Models.MyApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DatabridgeServer.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        // CREATE: Add Employee (Stored Procedure)

        public async Task<MessageResponse> AddEmployeeAsync(AddEmployeeRequest request)
        {
            var result = await _context.MessageResponses
                .FromSqlRaw("EXEC SP_AddEmployee @EmpName={0}, @DeptName={1}", request.EmpName, request.DeptName)
                .AsNoTracking()
                .ToListAsync();

            return result.FirstOrDefault() ?? new MessageResponse { Message = "Error executing stored procedure" };
        }

        // READ ALL: Get all employees with Dept details (Stored Procedure)
        public async Task<List<EmployeeFullResponse>> GetAllEmployeesFullAsync()
        {
            return await _context.EmployeeFullResponses
                .FromSqlRaw("EXEC SP_GetAllEmployeesFull")
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<EmployeeResult> GetEmployeeByIdAsync(int empId)
        {
            // Execute the Stored Procedure and map to List
            var result = await _context.EmployeeResults
                .FromSqlInterpolated($"EXEC SP_GetEmployeeById @EmpId = {empId}")
                .ToListAsync();

            // Return the first match, or null if empty
            return result.FirstOrDefault();
        }



        public async Task<string> UpdateEmployeeNameAsync(int empId, string empName)
        {
            string message = string.Empty;

            using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
            {
                using (var command = new SqlCommand("SP_UpdateEmployeeName", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@EmpId", empId);
                    command.Parameters.AddWithValue("@EmpName", empName);

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var messageValue = reader["Message"];
                            message = messageValue != DBNull.Value
                                ? messageValue.ToString() ?? string.Empty
                                : string.Empty;
                        }
                    }
                }
            }

            return message;
        }



        // DELETE: Delete Employee (Stored Procedure)
        public async Task<DeleteEmployeeResponse> DeleteEmployeeAsync(int empId)
        {
            var result = (await _context.DeleteEmployeeResponses
                .FromSqlRaw("EXEC SP_DeleteEmployee @EmpId={0}", empId)
                .AsNoTracking()
                .ToListAsync())
                .FirstOrDefault();

            return result ?? new DeleteEmployeeResponse
            {
                Message = "Error executing stored procedure"
            };
        }







    }
}


