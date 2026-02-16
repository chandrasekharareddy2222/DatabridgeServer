using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DatabridgeServer.Models
{
    public class Employee
    {

        [Key]
        [SwaggerIgnore]
        public int EmpId { get; set; }

        [Required]
        [StringLength(50)]
        public string EmpName { get; set; } = string.Empty; 

        [Required]
        [StringLength(50)]
        public string DeptName { get; set; } = string.Empty;
    }
    
    public class BulkImportResult
    {
        public int TotalRowsReceived { get; set; }
        public int SuccessfullyInserted { get; set; }
        public int Skipped { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }
    public class DeleteMultipleEmployeesRequest
    {
        [Required]
        public List<int> EmpIds { get; set; } = new();
    }
}