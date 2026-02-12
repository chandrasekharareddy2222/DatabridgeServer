using DatabridgeServer.Models;

namespace DatabridgeServer.Services.Students
{
    public interface IStudentService
    {
        Task<int> UploadStudentsFromExcelAsync(IFormFile file);

        Task<(int RowsDeleted, List<int> MissingIds)> DeleteStudentsBatchAsync(List<int> studentIds);
        Task<int> UploadStudentsFromCsvAsync(IFormFile file);
        Task InsertStudentAsync(Student student);
        Task<List<Student>> GetAllStudentsAsync();
        Task<Student?> GetStudentByIdAsync(int id);
        Task<bool> UpdateStudentAsync(int id, Student student);
        Task<bool> DeleteStudentAsync(int id);
    }
}
