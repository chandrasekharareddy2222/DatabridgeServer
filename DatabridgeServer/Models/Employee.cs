using Microsoft.AspNetCore.Mvc;
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

        public string EmpName { get; set; } = string.Empty; 

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
        public List<int> EmpIds { get; set; } = new();
        
    }

    // Wraps the route parameter for GET by ID and DELETE
    public class EmployeeIdRequest
    {
        [FromRoute(Name = "empId")] // Maps the {empId} from the URL to this property
        public int EmpId { get; set; }
    }

    // Wraps the IFormFile for Bulk Import
    public class FileImportRequest
    {
        public IFormFile File { get; set; }
    }
}