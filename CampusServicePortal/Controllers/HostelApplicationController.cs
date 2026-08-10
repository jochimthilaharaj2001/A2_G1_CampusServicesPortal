using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

        [HttpGet("mine")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<HostelApplicationModel>>> GetMyApplications()
        {
            var studentId = GetCurrentStudentId();
            if (studentId is null)
                return Forbid();

            return Ok(await _service.GetByStudentIdAsync(studentId.Value));
        }

        [HttpPost("mine")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<HostelApplicationModel>> CreateMyApplication(CreateHostelApplicationDto dto)
        {
            var studentId = GetCurrentStudentId();
            if (studentId is null)
                return Forbid();

            dto.StudentId = studentId.Value;
            return await CreateApplicationInternal(dto);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<HostelApplicationModel>>> GetApplications() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HostelApplicationModel>> GetApplication(int id)
        {
            var application = await _service.GetByIdAsync(id);
            return application is null ? NotFound() : Ok(application);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public Task<ActionResult<HostelApplicationModel>> CreateApplication(CreateHostelApplicationDto dto) =>
            CreateApplicationInternal(dto);

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HostelApplicationModel>> UpdateApplication(int id, UpdateHostelApplicationDto dto)
        {
            try
            {
                HostelApplicationModel? application;

                if (dto.RoomId.HasValue || string.Equals(dto.Status, "Room Assigned", StringComparison.OrdinalIgnoreCase))
                {
                    if (!dto.RoomId.HasValue)
                        return BadRequest(new { message = "Select a room before assigning it." });

                    application = await _service.AssignRoomAsync(id, new AssignHostelRoomDto { RoomId = dto.RoomId.Value });
                }
                else if (string.Equals(dto.Status, "Approved", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(dto.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    application = await _service.UpdateStatusAsync(id,
                        new UpdateHostelApplicationStatusDto { Status = dto.Status });
                }
                else
                {
                    application = await _service.UpdateAsync(id, dto);
                }

                return application is null ? NotFound() : Ok(application);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HostelApplicationModel>> UpdateStatus(
            int id,
            UpdateHostelApplicationStatusDto dto)
        {
            try
            {
                var application = await _service.UpdateStatusAsync(id, dto);
                return application is null ? NotFound() : Ok(application);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}/assign-room")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HostelApplicationModel>> AssignRoom(
            int id,
            AssignHostelRoomDto dto)
        {
            try
            {
                var application = await _service.AssignRoomAsync(id, dto);
                return application is null ? NotFound() : Ok(application);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            try
            {
                return await _service.DeleteAsync(id) ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private async Task<ActionResult<HostelApplicationModel>> CreateApplicationInternal(
            CreateHostelApplicationDto dto)
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

        private int? GetCurrentStudentId()
        {
            var claim = User.FindFirstValue("studentId");
            return int.TryParse(claim, out var studentId) && studentId > 0
                ? studentId
                : null;
        }
    }
}