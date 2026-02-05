using System.ComponentModel.DataAnnotations;

namespace DatabridgeServer.Models
{
    public class AddEmployeeRequest
    {
        [Required]
        public string EmpName { get; set; } = string.Empty;

        [Required]
        public string DeptName { get; set; } = string.Empty;
    }

    public class EmployeeResponse
    {
        public int? EmpId { get; set; }
        public string? EmpName { get; set; }
        public string? DeptName { get; set; }
        public string? Message { get; set; }
    }
}
