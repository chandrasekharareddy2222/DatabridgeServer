using DatabridgeServer.Models;
using DatabridgeServer.Services.Members;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DatabridgeServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _MemberService;

      

        public MemberController(IMemberService memberService)
        {
            _MemberService = memberService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllMembers()
        {
            var result = await _MemberService.GetAllMembersAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberById(int id)
        {
            var result = await _MemberService.GetMemberByIdAsync(id);

            if (result == null)
                return NotFound($"Member with ID {id} not found");

            return Ok(result);
        }



        [HttpPost]
        public async Task<IActionResult> PostMember(MemberBookDto dto)
        {
            var message = await _MemberService.InsertMemberAndBookAsync(dto);

            if (message.Contains("already"))
                return Conflict(new { message });

            if (message.Contains("error"))
                return StatusCode(500, new { message });

            return Ok(new { message });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMember(int id, [FromBody] MemberBookDto dto)
        {

            if (dto == null)
                return BadRequest("Request body cannot be empty");

          
            if (string.IsNullOrWhiteSpace(dto.MemberName))
                return BadRequest("MemberName is required");

            if (dto.MemberAge <= 0)
                return BadRequest("MemberAge must be greater than 0");

            if (string.IsNullOrWhiteSpace(dto.Bookname))
                return BadRequest("Bookname is required");

            
            var result = await _MemberService.UpdateMemberAsync(id, dto);

            if (result != "SUCCESS")
                return BadRequest(result);

            return Ok(new { message = "Member updated successfully" });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var result = await _MemberService.DeleteMemberAsync(id);

            if (result == "NOT FOUND")
                return NotFound($"Member with ID {id} not found");

            if (result != "SUCCESS")
                return BadRequest(result);

            return Ok(new { message = "Member deleted successfully" });
        }
        [HttpDelete("bulk")]
        public async Task<IActionResult> DeleteMultipleMembers([FromBody] DeleteMembersDto dto)
        {
            if (dto == null || dto.MemberIds == null || !dto.MemberIds.Any())
                return BadRequest("MemberIds are required");

            var deletedCount = await _MemberService.DeleteMembersAsync(dto.MemberIds);

            if (deletedCount == 0)
                return NotFound("No members found to delete");

            return Ok(new
            {
                message = $"{deletedCount} member(s) deleted successfully"
            });
        }

        


        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMembers([FromForm] UploadMemberDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("File is required");

            var extension = Path.GetExtension(dto.File.FileName).ToLower();

            if (extension != ".xlsx" && extension != ".csv")
                return BadRequest("Only XLSX and CSV files are allowed");

            var result = await _MemberService.ProcessFileAsync(dto.File);

            return Ok(result);
        }

    }






}
