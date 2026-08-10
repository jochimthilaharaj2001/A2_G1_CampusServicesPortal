using CampusServicePortal.Data;
using CampusServicesPortal.Hostel.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusServicesPortal.Hostel.Services
{
    public class NotificationQueue : INotificationQueue
    {
        private readonly ApplicationDbContext _context;

        public NotificationQueue(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task QueueForStudentAsync(int studentId, string type, string title, string message)
        {
            if (studentId <= 0)
            {
                return Task.CompletedTask;
            }

            _context.Notifications.Add(new Notification
            {
                StudentId = studentId,
                Type = type.Trim(),
                Title = title.Trim(),
                Message = message.Trim(),
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            });

            return Task.CompletedTask;
        }

        public async Task QueueForUserAsync(int userId, string type, string title, string message)
        {
            var studentId = await _context.Students
                .Where(student => student.UserId == userId)
                .Select(student => (int?)student.StudentId)
                .FirstOrDefaultAsync();

            if (studentId.HasValue)
            {
                await QueueForStudentAsync(studentId.Value, type, title, message);
            }
        }
    }
}