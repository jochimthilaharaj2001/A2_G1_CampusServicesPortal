using NotificationModel = CampusServicesPortal.Hostel.Models.Notification;
using CampusServicesPortal.Hostel.DTOs;

namespace CampusServicesPortal.Hostel.Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<NotificationModel>> GetAllAsync();
        Task<NotificationModel?> GetByIdAsync(int id);
        Task<IEnumerable<NotificationModel>> GetByStudentIdAsync(int studentId);
        Task<NotificationModel> CreateAsync(CreateNotificationDto dto);
        Task<NotificationModel?> UpdateAsync(int id, UpdateNotificationDto dto);
        Task<bool> DeleteAsync(int id);
        Task<NotificationModel?> MarkAsReadAsync(int id, int studentId);
        Task<bool> StudentExistsAsync(int studentId);
    }
}