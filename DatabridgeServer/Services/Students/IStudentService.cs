using DatabridgeServer.Models;

namespace DatabridgeServer.Services.Students
{
    public interface IStudentService
    {
        // POST (already working)
        Task InsertStudentAsync(Student student);

        // GET
        Task<List<Student>> GetAllStudentsAsync();
        Task<Student?> GetStudentByIdAsync(int id);

        // PUT
        Task<bool> UpdateStudentAsync(int id, Student student);

        // DELETE
        Task<bool> DeleteStudentAsync(int id);
    }
}
