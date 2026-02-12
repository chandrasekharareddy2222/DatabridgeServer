using DatabridgeServer.Models;

namespace DatabridgeServer.Services.Employees
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<(Employee employee, string message)> GetEmployeeByIdAsync(int empId);
        Task<string> AddEmployeeAsync(string empName, string deptName);
        Task<string> UpdateEmployeeAsync(int empId, string empName, string deptName);
        Task<string> DeleteEmployeeAsync(int empId);



    }
}