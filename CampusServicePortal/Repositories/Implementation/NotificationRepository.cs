using CampusServicePortal.Data;
using CampusServicesPortal.Hostel.DTOs;
using Microsoft.EntityFrameworkCore;
using NotificationModel = CampusServicesPortal.Hostel.Models.Notification;

namespace CampusServicesPortal.Hostel.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationModel>> GetAllAsync()
        {
            return await _context.Notifications
                .Include(n => n.Student)
                .ToListAsync();
        }

        public async Task<NotificationModel?> GetByIdAsync(int id)
        {
            return await _context.Notifications
                .Include(n => n.Student)
                .FirstOrDefaultAsync(n => n.NotificationId == id);
        }

        public async Task<IEnumerable<NotificationModel>> GetByStudentIdAsync(int studentId)
        {
            return await _context.Notifications
                .Where(n => n.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<NotificationModel> CreateAsync(CreateNotificationDto dto)
        {
            var notification = new NotificationModel
            {
                StudentId = dto.StudentId,
                Title = dto.Title,
                Message = dto.Message,
                IsRead = false,
                CreatedDate = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<NotificationModel?> UpdateAsync(
            int id,
            UpdateNotificationDto dto)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return null;

            notification.Title = dto.Title;
            notification.Message = dto.Message;
            notification.IsRead = dto.IsRead;

            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> StudentExistsAsync(int studentId)
        {
            return await _context.Students
                .AnyAsync(s => s.StudentId == studentId);
        }
    }
}