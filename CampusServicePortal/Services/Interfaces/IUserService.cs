using CampusServicePortal.DTOs.Auth;
using CampusServicePortal.DTOs.Users;

namespace CampusServicePortal.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        Task<UserDto?> GetUserByIdAsync(int id);
    }
}