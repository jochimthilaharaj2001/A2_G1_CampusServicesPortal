using CampusServicePortal.Models;

namespace CampusServicePortal.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);

        Task<User?> GetUserByIdAsync(int userId);

        Task AddUserAsync(User user);

        Task AddStudentAsync(Student student);

        Task SaveChangesAsync();
    }
}