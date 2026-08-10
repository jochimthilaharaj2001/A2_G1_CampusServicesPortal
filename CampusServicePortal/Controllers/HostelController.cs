using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        public async Task<ActionResult<IEnumerable<HostelModel>>> GetHostels() =>
            Ok(await _hostelService.GetAllAsync());

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<HostelModel>> GetHostel(int id)
        {
            var hostel = await _hostelService.GetByIdAsync(id);
            return hostel is null ? NotFound() : Ok(hostel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HostelModel>> UpdateHostel(int id, UpdateHostelDto dto)
        {
            try
            {
                var hostel = await _hostelService.UpdateAsync(id, dto);
                return hostel is null ? NotFound() : Ok(hostel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHostel(int id)
        {
            try
            {
                return await _hostelService.DeleteAsync(id) ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}