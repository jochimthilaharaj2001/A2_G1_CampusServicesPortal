using CampusServicesPortal.Hostel.DTOs;
using NotificationModel = CampusServicesPortal.Hostel.Models.Notification;

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

        Task<bool> StudentExistsAsync(int studentId);
    }
}