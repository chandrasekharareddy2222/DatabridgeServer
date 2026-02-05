using DatabridgeServer.Models;
using System.Threading.Tasks;

namespace DatabridgeServer.Services
{
    public interface IEmployeeService
    {
        Task<EmployeeResponse> AddEmployeeAsync(AddEmployeeRequest request);
    }
}
