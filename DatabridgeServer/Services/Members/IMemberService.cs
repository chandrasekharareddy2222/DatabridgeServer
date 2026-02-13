using DatabridgeServer.Models;

namespace DatabridgeServer.Services.Members
{
    public interface IMemberService
    {
        Task<string> InsertMemberAndBookAsync(MemberBookDto memberBookDto);
    }
}
