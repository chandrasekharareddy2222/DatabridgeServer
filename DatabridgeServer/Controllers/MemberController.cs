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

        public MemberController(IMemberService MemberService)
        {
            _MemberService = MemberService;
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




    }
}