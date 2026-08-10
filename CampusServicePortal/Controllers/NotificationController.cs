using Microsoft.AspNetCore.Mvc;
using CampusServicesPortal.Hostel.DTOs;
using CampusServicesPortal.Hostel.Services;
using NotificationModel = CampusServicesPortal.Hostel.Models.Notification;

namespace CampusServicesPortal.Hostel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationModel>>> GetNotifications()
        {
            var notifications = await _service.GetAllAsync();
            return Ok(notifications);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationModel>> GetNotification(int id)
        {
            var notification = await _service.GetByIdAsync(id);

            if (notification == null)
                return NotFound();

            return Ok(notification);
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<NotificationModel>>> GetByStudentId(int studentId)
        {
            try
            {
                var notifications = await _service.GetByStudentIdAsync(studentId);
                return Ok(notifications);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<NotificationModel>> CreateNotification(
            CreateNotificationDto dto)
        {
            try
            {
                var notification = await _service.CreateAsync(dto);

                return CreatedAtAction(
                    nameof(GetNotification),
                    new { id = notification.NotificationId },
                    notification);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<NotificationModel>> UpdateNotification(
            int id,
            UpdateNotificationDto dto)
        {
            try
            {
                var notification = await _service.UpdateAsync(id, dto);

                if (notification == null)
                    return NotFound();

                return Ok(notification);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}