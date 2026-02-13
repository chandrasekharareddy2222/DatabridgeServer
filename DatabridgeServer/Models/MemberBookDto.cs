using System.ComponentModel.DataAnnotations;

namespace DatabridgeServer.Models
{
    public class MemberBookDto
    {
        

        [Required]
        public string Bookname { get; set; }

        [Required]
        public string MemberName { get; set; }

        [Range(1, 79, ErrorMessage = "Member age must be less than 80")]
        public int MemberAge {  get; set; }
    }
}
