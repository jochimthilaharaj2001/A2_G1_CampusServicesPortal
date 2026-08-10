using CampusServicePortal.Data;
using Microsoft.EntityFrameworkCore;
using CampusServicesPortal.Hostel.DTOs;
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
                .AsNoTracking()
                .Include(notification => notification.Student)
                .OrderByDescending(notification => notification.CreatedDate)
                .ToListAsync();
        }

        public Task<NotificationModel?> GetByIdAsync(int id) =>
            _context.Notifications.FirstOrDefaultAsync(notification => notification.NotificationId == id);

        public async Task<NotificationModel> CreateAsync(CreateNotificationDto dto)
        {
            var notification = new NotificationModel
            {
                StudentId = dto.StudentId,
                Title = dto.Title.Trim(),
                Message = dto.Message.Trim(),
                Type = string.IsNullOrWhiteSpace(dto.Type) ? "System" : dto.Type.Trim(),
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<NotificationModel?> UpdateAsync(int id, UpdateNotificationDto dto)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(item => item.NotificationId == id);
            if (notification is null)
                return null;

            notification.Title = dto.Title.Trim();
            notification.Message = dto.Message.Trim();
            notification.IsRead = dto.IsRead;
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification is null)
                return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<NotificationModel>> GetByStudentIdAsync(int studentId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(notification => notification.StudentId == studentId)
                .OrderByDescending(notification => notification.CreatedDate)
                .ToListAsync();
        }

        public async Task<NotificationModel?> MarkAsReadAsync(int id, int studentId)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(item =>
                item.NotificationId == id && item.StudentId == studentId);

            if (notification == null)
                return null;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return notification;
        }

        public Task<bool> StudentExistsAsync(int studentId) =>
            _context.Students.AnyAsync(student => student.StudentId == studentId);
    }
}