using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CampusServicesPortal.Hostel.Services;
using CampusServicesPortal.Hostel.DTOs;
using NotificationModel = CampusServicesPortal.Hostel.Models.Notification;

namespace CampusServicesPortal.Hostel.Controllers
{
    [Route("api/notifications")]
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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<NotificationModel>>> GetNotifications() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<NotificationModel>> GetNotification(int id)
        {
            var notification = await _service.GetByIdAsync(id);
            return notification is null ? NotFound() : Ok(notification);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<NotificationModel>> CreateNotification(CreateNotificationDto dto)
        {
            try
            {
                var notification = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetNotification), new { id = notification.NotificationId }, notification);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<NotificationModel>> UpdateNotification(int id, UpdateNotificationDto dto)
        {
            try
            {
                var notification = await _service.UpdateAsync(id, dto);
                return notification is null ? NotFound() : Ok(notification);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteNotification(int id) =>
            await _service.DeleteAsync(id) ? NoContent() : NotFound();
        [HttpGet("mine")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<NotificationModel>>> GetMyNotifications()
        {
            var studentId = GetCurrentStudentId();
            return studentId is null
                ? Forbid()
                : Ok(await _service.GetByStudentIdAsync(studentId.Value));
        }

        [HttpPut("{id:int}/read")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<NotificationModel>> MarkAsRead(int id)
        {
            var studentId = GetCurrentStudentId();
            if (studentId is null)
                return Forbid();

            var notification = await _service.MarkAsReadAsync(id, studentId.Value);
            return notification is null ? NotFound() : Ok(notification);
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