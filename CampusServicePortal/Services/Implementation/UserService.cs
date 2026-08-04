using CampusServicePortal.DTOs.Users;
using CampusServicePortal.Repositories.Interfaces;
using CampusServicePortal.Services.Interfaces;

namespace CampusServicePortal.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;


        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }



        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();


            return users.Select(user => new UserDto
            {
                UserId = user.UserId,

                FullName = user.FullName,

                Email = user.Email,

                RoleName = user.Role.RoleName

            });
        }





        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);


            if (user == null)
            {
                return null;
            }


            return new UserDto
            {
                UserId = user.UserId,

                FullName = user.FullName,

                Email = user.Email,

                RoleName = user.Role.RoleName
            };
        }
    }
}