using CampusServicePortal.DTOs.Auth;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CampusServicePortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {

            try
            {
                var result =
                    await _authService.RegisterAsync(dto);


                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }

        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {

            var result =
                await _authService.LoginAsync(dto);


            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password"
                });
            }


            return Ok(result);
        }



    }
}
