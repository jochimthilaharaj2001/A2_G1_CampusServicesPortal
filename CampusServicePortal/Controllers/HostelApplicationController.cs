using Microsoft.AspNetCore.Mvc;
using CampusServicesPortal.Hostel.DTOs;
using CampusServicesPortal.Hostel.Services;
using HostelApplicationModel = CampusServicesPortal.Hostel.Models.HostelApplication;

namespace CampusServicesPortal.Hostel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HostelApplicationController : ControllerBase
    {
        private readonly IHostelApplicationService _service;

        public HostelApplicationController(IHostelApplicationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HostelApplicationModel>>> GetApplications()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HostelApplicationModel>> GetApplication(int id)
        {
            var application = await _service.GetByIdAsync(id);

            if (application == null)
                return NotFound();

            return Ok(application);
        }

        [HttpPost]
        public async Task<ActionResult<HostelApplicationModel>> CreateApplication(CreateHostelApplicationDto dto)
        {
            try
            {
                var application = await _service.CreateAsync(dto);

                return CreatedAtAction(nameof(GetApplication),
                    new { id = application.ApplicationId }, application);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<HostelApplicationModel>> UpdateApplication(int id, UpdateHostelApplicationDto dto)
        {
            try
            {
                var application = await _service.UpdateAsync(id, dto);

                if (application == null)
                    return NotFound();

                return Ok(application);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}