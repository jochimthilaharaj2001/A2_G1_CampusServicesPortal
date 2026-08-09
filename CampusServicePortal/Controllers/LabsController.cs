using CampusServicePortal.DTOs.Labs;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusServicePortal.Controllers
{
    [Route("api/labs")]
    [ApiController]
    [Authorize]
    public class LabsController : ControllerBase
    {
        private readonly ILabService _labService;

        public LabsController(ILabService labService)
        {
            _labService = labService;
        }

        // GET: api/labs
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var labs = await _labService.GetAllLabsAsync();
            return Ok(labs);
        }

        // GET: api/labs/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var lab = await _labService.GetLabByIdAsync(id);
            if (lab == null)
            {
                return NotFound(new { message = $"Lab with ID {id} was not found." });
            }
            return Ok(lab);
        }

        // GET: api/labs/{id}/slots?date=2026-08-08
        [HttpGet("{id:int}/slots")]
        public async Task<IActionResult> GetAvailableSlots(int id, [FromQuery] DateTime date)
        {
            if (date == default)
            {
                return BadRequest(new { message = "A valid date must be provided." });
            }

            var slots = await _labService.GetAvailableSlotsAsync(id, date);
            return Ok(slots);
        }

        // POST: api/labs (admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateLabDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _labService.CreateLabAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.LabId }, result);
        }

        // PUT: api/labs/{id} (admin only)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLabDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _labService.UpdateLabAsync(id, dto);
            if (result == null)
            {
                return NotFound(new { message = $"Lab with ID {id} was not found." });
            }

            return Ok(result);
        }

        // DELETE: api/labs/{id} (admin only)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _labService.DeleteLabAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Lab with ID {id} was not found." });
            }

            return Ok(new { message = "Lab deactivated successfully." });
        }

        // POST: api/labs/{id}/seats (admin only)
        [HttpPost("{id:int}/seats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSeat(int id, [FromBody] CreateLabSeatDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _labService.AddSeatAsync(id, dto);
                if (result == null)
                {
                    return NotFound(new { message = $"Lab with ID {id} was not found." });
                }
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/labs/{id}/seats/{seatId} (admin only)
        [HttpDelete("{id:int}/seats/{seatId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveSeat(int id, int seatId)
        {
            try
            {
                var success = await _labService.RemoveSeatAsync(id, seatId);
                if (!success)
                {
                    return NotFound(new { message = "Seat not found." });
                }
                return Ok(new { message = "Seat removed successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/labs/{id}/seats?date=2026-08-08&slot=09:00:00
        [HttpGet("{id:int}/seats")]
        public async Task<IActionResult> GetSeatAvailability(int id, [FromQuery] DateTime date, [FromQuery] TimeSpan slot)
        {
            if (date == default || slot == default)
            {
                return BadRequest(new { message = "Valid date and slot time must be provided." });
            }

            var availability = await _labService.GetSeatAvailabilityAsync(id, date, slot);
            return Ok(availability);
        }
    }
}
