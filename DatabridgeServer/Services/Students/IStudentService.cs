using DatabridgeServer.Models;

namespace DatabridgeServer.Services.Students
{
    public interface IStudentService
    {
        Task InsertStudentAsync(Student student);
    }
}
