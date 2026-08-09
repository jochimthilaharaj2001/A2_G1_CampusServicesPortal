using System.Security.Claims;
using CampusServicePortal.DTOs.LabReservation;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusServicePortal.Controllers
{
    [Route("api/lab-bookings")]
    [ApiController]
    [Authorize]
    public class LabReservationsController : ControllerBase
    {
        private readonly ILabReservationService _service;

        public LabReservationsController(ILabReservationService service)
        {
            _service = service;
        }

        // GET: api/lab-bookings
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var reservations = await _service.GetAllAsync();
            return Ok(reservations);
        }

        // GET: api/lab-bookings/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await _service.GetByIdAsync(id);
            if (reservation == null)
            {
                return NotFound(new { message = "Lab reservation not found." });
            }

            // Only owner or Admin can view
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            if (userRole != "Admin" && reservation.UserId != userId)
            {
                return Forbid();
            }

            return Ok(reservation);
        }

        // POST: api/lab-bookings
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLabReservationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Automatically set UserId to current user if not Admin, or validate owner
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            if (userRole != "Admin")
            {
                dto.UserId = userId;
            }

            try
            {
                var result = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.LabReservationId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/lab-bookings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLabReservationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reservation = await _service.GetByIdAsync(id);
            if (reservation == null)
            {
                return NotFound(new { message = "Lab reservation not found." });
            }

            // Only owner or Admin can update
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            if (userRole != "Admin" && reservation.UserId != userId)
            {
                return Forbid();
            }

            try
            {
                var result = await _service.UpdateAsync(id, dto);
                if (result == null)
                {
                    return NotFound(new { message = "Lab reservation not found." });
                }
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/lab-bookings/5 (Cancel reservation - owner only or admin)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            try
            {
                var success = await _service.CancelReservationAsync(id, userId, userRole);
                if (!success)
                {
                    return NotFound(new { message = "Lab reservation not found." });
                }

                return Ok(new { message = "Lab reservation cancelled successfully." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/lab-bookings/student/{studentId}
        [HttpGet("student/{studentId:int}")]
        public async Task<IActionResult> GetByStudentId(int studentId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            // Admin can view any student's bookings.
            // For students, we should verify that this studentId belongs to the current user's profile.
            // We can retrieve the student bookings and filter/verify.
            var bookings = await _service.GetByStudentIdAsync(studentId);

            if (userRole != "Admin")
            {
                // Verify the owner
                var isOwner = bookings.All(b => b.UserId == userId);
                if (!isOwner && bookings.Any())
                {
                    return Forbid();
                }
            }

            return Ok(bookings);
        }

        private int GetCurrentUserId()
        {
            var claimVal = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            int.TryParse(claimVal, out int id);
            return id;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role") ?? string.Empty;
        }
    }
}