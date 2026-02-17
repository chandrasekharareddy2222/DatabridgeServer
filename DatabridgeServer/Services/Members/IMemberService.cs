using DatabridgeServer.Models;

namespace DatabridgeServer.Services.Members
{
    public interface IMemberService
    {
        Task<List<MemberBookDto>> GetAllMembersAsync();

        Task<MemberBookDto?> GetMemberByIdAsync(int memberId);

        Task<string> InsertMemberAndBookAsync(MemberBookDto memberBookDto);
        Task<string> UpdateMemberAsync(int memberId, MemberBookDto dto);
        Task<string> DeleteMemberAsync(int memberId);
        Task<int> DeleteMembersAsync(List<int> memberIds);

      
        Task<List<string>> ProcessFileAsync(IFormFile file);





    }
}
