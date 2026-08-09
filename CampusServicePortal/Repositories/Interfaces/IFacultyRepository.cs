using CampusServicePortal.Models;

namespace CampusServicePortal.Repositories.Interfaces
{
    public interface IFacultyRepository
    {
        Task<IEnumerable<Faculty>> GetAllAsync(bool activeOnly = false);
        Task<Faculty?> GetByIdAsync(int id);
        Task<Faculty?> GetByNameAsync(string name);
        Task AddAsync(Faculty faculty);
        Task UpdateAsync(Faculty faculty);
        Task SaveChangesAsync();
        Task<int> CountStudentsAsync(int facultyId);
    }
}
