using System.ComponentModel.DataAnnotations;

namespace DatabridgeServer.Models
{
    public class Student
    {
        public int StudentID { get; set; }

        [Required]
        [StringLength(100)]
        public string StudentName { get; set; } = string.Empty;

        [Required]
        [Range(18, 60)]
        public int Age { get; set; }

        [Required]
        [StringLength(100)]
        public string DeptName { get; set; } = string.Empty;
    }
}
