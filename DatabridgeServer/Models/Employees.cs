
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DatabridgeServer.Models
{
    public class EmployeeFullResponse
    {
        public int EmpId { get; set; }
        public string? EmpName { get; set; }
        public int DeptId { get; set; }
        public string? DeptName { get; set; }
    }
     
    public class EmployeeResponse
    {
        public int? EmpId { get; set; }
        public string? EmpName { get; set; }
        public string? DeptName { get; set; }
        public string? Message { get; set; }
    }

    public class EmployeeByIdResponse
    {
        public string? EmpName { get; set; }
        public string? DeptName { get; set; }
        public string? Message { get; set; }

    }
    public class AddEmployeeRequest
    {
        [Required(ErrorMessage = "EmpName is required")]
        public string EmpName { get; set; } = string.Empty;

        [Required(ErrorMessage = "DeptName is required")]
        public string DeptName { get; set; } = string.Empty;
    }
    public class DeleteEmployeeResponse
    {
        public string? Message { get; set; }
    }

    public class UpdateEmployeeRequest
    {
        public string EmpName { get; set; }
    }

    [Keyless]
    public class MessageResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    namespace MyApi.Models
    {
        public class EmployeeResult
        {
            public string EmpName { get; set; }
            public string DeptName { get; set; }
        }
    }
}

