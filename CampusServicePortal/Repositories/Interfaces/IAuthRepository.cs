using CampusServicePortal.Models;

namespace CampusServicePortal.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(int userId);
        Task AddUserAsync(User user);
        Task AddStudentAsync(Student student);
        Task UpdateUserAsync(User user);
        Task<User?> GetUserByVerificationTokenAsync(string token);
        Task SaveChangesAsync();
    }
}