
using DatabridgeServer.Models;
using DatabridgeServer.Models.MyApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabridgeServer.Services
{
    public interface IEmployeeService
    {
        Task<MessageResponse> AddEmployeeAsync(AddEmployeeRequest request);
        Task<List<EmployeeFullResponse>> GetAllEmployeesFullAsync();
        Task<EmployeeResult> GetEmployeeByIdAsync(int empId);
        Task<string> UpdateEmployeeNameAsync(int empId, string empName);
        Task<DeleteEmployeeResponse> DeleteEmployeeAsync(int empId);
        
    }
}