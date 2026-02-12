using DatabridgeServer.Data;
using DatabridgeServer.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
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
                            EmpName = reader["EmpName"].ToString(),
                            DeptName = reader["DeptName"].ToString()
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





    }
}