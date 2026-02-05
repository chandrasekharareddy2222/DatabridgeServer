using DatabridgeServer.Data;
using DatabridgeServer.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabridgeServer.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        private static readonly Regex AllowedCharactersRegex =
            new(@"^[a-zA-Z_\-][a-zA-Z0-9\s\-_]*$");

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeResponse> AddEmployeeAsync(AddEmployeeRequest request)
        {
            // Business validations
            if (request == null)
                return new EmployeeResponse { Message = "Request body is required." };

            if (string.IsNullOrWhiteSpace(request.EmpName))
                return new EmployeeResponse { Message = "EmpName is required and cannot be empty." };

            if (string.IsNullOrWhiteSpace(request.DeptName))
                return new EmployeeResponse { Message = "DeptName is required and cannot be empty." };

            if (!AllowedCharactersRegex.IsMatch(request.EmpName))
                return new EmployeeResponse { Message = "EmpName contains invalid characters." };

            if (!AllowedCharactersRegex.IsMatch(request.DeptName))
                return new EmployeeResponse { Message = "DeptName contains invalid characters." };

            var empNameParam = new SqlParameter("@EmpName", SqlDbType.VarChar, 50)
            {
                Value = request.EmpName
            };

            var deptNameParam = new SqlParameter("@DeptName", SqlDbType.VarChar, 50)
            {
                Value = request.DeptName
            };

            var result = await _context.EmployeeResponses
                .FromSqlRaw("EXEC SP_AddEmployee @EmpName, @DeptName",
                            empNameParam, deptNameParam)
                .ToListAsync();

            if (result.Count == 0)
                return new EmployeeResponse { Message = "No response from stored procedure." };

            return result[0];
        }
    }
}
