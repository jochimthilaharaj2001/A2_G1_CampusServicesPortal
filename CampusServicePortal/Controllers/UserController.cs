using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CampusServicePortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/users

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {

            var users =
                await _userService.GetAllUsersAsync();


            return Ok(users);

        }

        // GET: api/users/5

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {

            var user =
                await _userService.GetUserByIdAsync(id);



            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found"
                });
            }


            return Ok(user);

        }



    }
}
