using DatabridgeServer.Data;
using DatabridgeServer.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.IO;
using System.Text.RegularExpressions;
namespace DatabridgeServer.Services.Employees
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees
                .FromSqlRaw("EXEC SP_GetAllEmployeesFull")
                .ToListAsync();
        }
        public async Task<(Employee employee, string message)> GetEmployeeByIdAsync(int empId)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SP_GetEmployeeById";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@EmpId", empId));

                    if (connection.State != ConnectionState.Open)
                        await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                            return (null, "No data returned");
                        await reader.ReadAsync();
                        if (reader.FieldCount == 1)
                        {
                            return (null, reader.GetString(0));
                        }

                        var employee = new Employee
                        {
                            EmpId = empId,
                            EmpName = reader["EmpName"]?.ToString() ?? "",
                            DeptName = reader["DeptName"]?.ToString() ?? ""
                        };

                        return (employee, null);
                    }
                }
            }
        }
        public async Task<string> AddEmployeeAsync(string empName, string deptName)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SP_AddEmployee";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@EmpName", empName));
                    command.Parameters.Add(new SqlParameter("@DeptName", deptName));

                    if (connection.State != ConnectionState.Open)
                        await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return reader["Message"].ToString();
                        }
                    }

                    return "No response from database";
                }
            }
        }

        public async Task<string> UpdateEmployeeAsync(int empId, string empName, string deptName)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SP_UpdateEmployeeName";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@EmpId", empId));
                    command.Parameters.Add(new SqlParameter("@EmpName", empName));
                    command.Parameters.Add(new SqlParameter("@DeptName", deptName));

                    if (connection.State != ConnectionState.Open)
                        await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return reader["Message"].ToString();
                        }
                    }

                    return "No response from database";
                }
            }
        }

        public async Task<string> DeleteEmployeeAsync(int empId)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SP_DeleteEmployee";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@EmpId", empId));

                    if (connection.State != ConnectionState.Open)
                        await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return reader["Message"].ToString();
                        }
                    }

                    return "No response from database";
                }
            }
        }

        public async Task<BulkImportResult> BulkImportEmployeesAsync(IFormFile file)
        {
            var result = new BulkImportResult();
            var validEmployeesTable = new DataTable();
            validEmployeesTable.Columns.Add("EmpName", typeof(string));
            validEmployeesTable.Columns.Add("DeptName", typeof(string));
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    if (worksheet.Dimension == null) return result;

                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var empNameRaw = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var deptNameRaw = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

                        if (string.IsNullOrWhiteSpace(empNameRaw) ||
                            string.IsNullOrWhiteSpace(deptNameRaw) ||
                            empNameRaw.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                            deptNameRaw.Equals("null", StringComparison.OrdinalIgnoreCase))
                        {
                            result.ValidationErrors.Add($"Row {row}: Skipped (Empty or Null values)");
                            continue;
                        }
                        if (!Regex.IsMatch(empNameRaw, "^[a-zA-Z]") || !Regex.IsMatch(deptNameRaw, "^[a-zA-Z]"))
                        {
                            result.ValidationErrors.Add($"Row {row}: Skipped (Names must start with a letter)");
                            continue;
                        }
                        validEmployeesTable.Rows.Add(empNameRaw, deptNameRaw);
                    }
                }
            }
            if (validEmployeesTable.Rows.Count > 0)
            {
                using (var connection = _context.Database.GetDbConnection())
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SP_BulkImportEmployees";
                    command.CommandType = CommandType.StoredProcedure;

                    var parameter = new SqlParameter("@Employees", validEmployeesTable)
                    {
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "EmployeeImportType"
                    };
                    command.Parameters.Add(parameter);

                    if (connection.State != ConnectionState.Open)
                        await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result.TotalRowsReceived = reader.GetInt32(0);
                            result.SuccessfullyInserted = reader.GetInt32(1);
                            int dbSkipped = reader.GetInt32(2);
                            result.Skipped = dbSkipped + result.ValidationErrors.Count;

                            result.Message = reader.GetString(3);
                        }
                    }
                }
            }
            else
            {
                result.Message = "No valid rows found to process.";
                result.Skipped = result.ValidationErrors.Count;
            }

            return result;
        }

        public async Task<string> DeleteMultipleEmployeesAsync(List<int> empIds)
        {
            if (empIds == null || empIds.Count == 0)
                return "No Employee IDs provided.";
            DataTable table = new DataTable();
            table.Columns.Add("EmpId", typeof(int));

            foreach (var id in empIds.Distinct())
            {
                table.Rows.Add(id);
            }
            using var connection = _context.Database.GetDbConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SP_DeleteMultipleEmployees";
            command.CommandType = CommandType.StoredProcedure;

            var parameter = new SqlParameter("@EmpIds", SqlDbType.Structured)
            {
                TypeName = "EmpIdTableType",
                Value = table
            };
            command.Parameters.Add(parameter);
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            return result?.ToString() ?? "Operation completed.";
        }
    }
}