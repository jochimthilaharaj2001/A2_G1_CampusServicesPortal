using CampusServicePortal.Models;

namespace CampusServicePortal.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<User?> GetUserByIdAsync(int userId);

        Task<User?> GetUserByEmailAsync(string email);

        Task UpdateUserAsync(User user);

        Task DeleteUserAsync(User user);

        Task SaveChangesAsync();
    }
}