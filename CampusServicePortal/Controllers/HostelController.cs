
using Microsoft.AspNetCore.Mvc;
using CampusServicesPortal.Hostel.Services;
using CampusServicesPortal.Hostel.DTOs;
using HostelModel = CampusServicesPortal.Hostel.Models.Hostel;

namespace CampusServicesPortal.Hostel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HostelController : ControllerBase
    {
        private readonly IHostelService _hostelService;

        public HostelController(IHostelService hostelService)
        {
            _hostelService = hostelService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HostelModel>>> GetHostels()
        {
            var hostels = await _hostelService.GetAllAsync();
            return Ok(hostels);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<HostelModel>> GetHostel(int id)
        {
            var hostel = await _hostelService.GetByIdAsync(id);

            if (hostel == null)
            {
                return NotFound();
            }

            return Ok(hostel);
        }
        [HttpPost]
        public async Task<ActionResult<HostelModel>> CreateHostel(CreateHostelDto dto)
        {
            try
            {
                var hostel = await _hostelService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetHostel), new { id = hostel.HostelId }, hostel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<HostelModel>> UpdateHostel(int id, UpdateHostelDto dto)
        {
            try
            {
                var hostel = await _hostelService.UpdateAsync(id, dto);

                if (hostel == null)
                {
                    return NotFound();
                }

                return Ok(hostel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHostel(int id)
        {
            var deleted = await _hostelService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}