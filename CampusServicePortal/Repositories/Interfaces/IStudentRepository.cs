using CampusServicePortal.Models;

namespace CampusServicePortal.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        Task<Student?> GetByIdAsync(int studentId);
        Task<Student?> GetByUserIdAsync(int userId);
        Task<Student?> GetByIndexNumberAsync(string indexNumber);
        Task<(IEnumerable<Student> Items, int TotalCount)> GetAllAsync(
            string? search, string? faculty, int page, int pageSize);
        Task UpdateAsync(Student student);
        Task SaveChangesAsync();
    }
}
