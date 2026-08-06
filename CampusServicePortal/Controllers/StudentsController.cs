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

        public StudentsController(IStudentService studentService, IAuthService authService)
        {
            _studentService = studentService;
            _authService = authService;
        }

        private readonly IAuthService _authService;

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

        // GET: api/students/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetProfile(int id)
        {
            try
            {
                var profile = await _studentService.GetProfileAsync(id);
                if (profile == null)
                    return NotFound(new { message = $"Student with ID {id} was not found." });

                // Non-admin students can only view their own profile
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                
                if (userRole == "Student" && userIdClaim != null)
                {
                    if (int.TryParse(userIdClaim, out int currentUserId) && currentUserId != profile.UserId)
                    {
                        return Forbid();
                    }
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/students/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileDto dto)
        {
            try
            {
                var existingProfile = await _studentService.GetProfileAsync(id);
                if (existingProfile == null)
                    return NotFound(new { message = $"Student with ID {id} was not found." });

                // Students can only update their own profile
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

                if (userRole == "Student" && userIdClaim != null)
                {
                    if (int.TryParse(userIdClaim, out int currentUserId) && currentUserId != existingProfile.UserId)
                    {
                        return Forbid();
                    }
                }

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

        // DELETE: api/students/{id} (Admin only soft delete / deactivation)
        [HttpDelete("{id}")]
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

        // PUT: api/admin/students/{id}/deactivate
        [HttpPut("/api/admin/students/{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDeactivateStudent(int id)
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
        }

        // PUT: api/admin/students/{id}/reactivate
        [HttpPut("/api/admin/students/{id}/reactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminReactivateStudent(int id)
        {
            try
            {
                await _studentService.ReactivateStudentAsync(id);
                return Ok(new { message = "Student account reactivated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/admin/students/{id}/deactivation-check
        [HttpGet("/api/admin/students/{id}/deactivation-check")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckDeactivation(int id)
        {
            try
            {
                var blockers = await _studentService.CheckDeactivationBlockersAsync(id);
                return Ok(new
                {
                    canDeactivate = !blockers.Any(),
                    blockingReasons = blockers
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
