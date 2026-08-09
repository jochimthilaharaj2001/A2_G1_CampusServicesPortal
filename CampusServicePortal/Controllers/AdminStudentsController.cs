using CampusServicePortal.DTOs.Students;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusServicePortal.Controllers
{
    /// <summary>
    /// BRD Module 1 — Admin student management endpoints under /api/admin/students.
    /// </summary>
    [Route("api/admin/students")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminStudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public AdminStudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // POST: api/admin/students
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdminCreateStudentDto dto)
        {
            try
            {
                var result = await _studentService.CreateStudentByAdminAsync(dto);
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

        // PUT: api/admin/students/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AdminUpdateStudentDto dto)
        {
            try
            {
                var updated = await _studentService.UpdateStudentByAdminAsync(id, dto);
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

        // PUT: api/admin/students/{id}/deactivate
        [HttpPut("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
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
        [HttpPut("{id:int}/reactivate")]
        public async Task<IActionResult> Reactivate(int id)
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
        [HttpGet("{id:int}/deactivation-check")]
        public async Task<IActionResult> DeactivationCheck(int id)
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
