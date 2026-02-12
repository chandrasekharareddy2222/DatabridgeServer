using System.ComponentModel.DataAnnotations;

namespace DatabridgeServer.Models
{
    public class Employee
    {

        [Required]
        [StringLength(50)]
        public string EmpName { get; set; } = string.Empty;


        [Required]
        [StringLength(50)]
        public string DeptName { get; set; } = string.Empty;
    }
}