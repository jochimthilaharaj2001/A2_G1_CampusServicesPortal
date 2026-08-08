using CampusServicePortal.DTOs.Students;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusServicePortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IAuthService _authService;

        public StudentsController(IStudentService studentService, IAuthService authService)
        {
            _studentService = studentService;
            _authService = authService;
        }

        // POST: api/students/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterStudent([FromBody] CampusServicePortal.DTOs.Auth.RegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                return Created(string.Empty, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/students/me — current authenticated student's profile
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var profile = await ResolveCurrentStudentProfileAsync();
                if (profile == null)
                    return NotFound(new { message = "No student profile is linked to this account." });

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/students/{id}
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetProfile(int id)
        {
            try
            {
                var profile = await _studentService.GetProfileAsync(id);
                if (profile == null)
                    return NotFound(new { message = $"Student with ID {id} was not found." });

                if (!CanAccessStudent(profile.UserId))
                    return Forbid();

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/students/{id}
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileDto dto)
        {
            try
            {
                var existingProfile = await _studentService.GetProfileAsync(id);
                if (existingProfile == null)
                    return NotFound(new { message = $"Student with ID {id} was not found." });

                if (!CanAccessStudent(existingProfile.UserId))
                    return Forbid();

                var updated = await _studentService.UpdateProfileAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/students?search=&faculty=&page=&pageSize=
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchStudents(
            [FromQuery] string? search,
            [FromQuery] string? faculty,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var (items, totalCount) = await _studentService.SearchStudentsAsync(search, faculty, page, pageSize);
                return Ok(new
                {
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    items
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/students/{id} — soft deactivate (admin); BRD also exposes PUT deactivate
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateStudent(int id)
        {
            try
            {
                await _studentService.DeactivateStudentAsync(id);
                return Ok(new { message = "Student account deactivated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private bool CanAccessStudent(int profileUserId)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole == "Admin")
                return true;

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(userIdClaim, out int currentUserId) && currentUserId == profileUserId;
        }

        private async Task<StudentProfileDto?> ResolveCurrentStudentProfileAsync()
        {
            var studentIdClaim = User.FindFirstValue("studentId");
            if (int.TryParse(studentIdClaim, out int studentId) && studentId > 0)
            {
                var byStudentId = await _studentService.GetProfileAsync(studentId);
                if (byStudentId != null)
                    return byStudentId;
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (int.TryParse(userIdClaim, out int userId))
                return await _studentService.GetProfileByUserIdAsync(userId);

            return null;
        }
    }
}
