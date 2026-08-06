using CampusServicePortal.Models;

namespace CampusServicePortal.Repositories.Interfaces
{
    public interface IStudentMasterListRepository
    {
        Task<StudentMasterList?> GetByIndexNumberAsync(string indexNumber);
        Task<IEnumerable<StudentMasterList>> GetAllAsync(string? search);
        Task AddRangeAsync(IEnumerable<StudentMasterList> records);
        Task MarkAsRegisteredAsync(string indexNumber);
        Task SaveChangesAsync();
    }
}
