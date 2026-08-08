using CampusServicesPortal.Hostel.DTOs;
using NotificationModel = CampusServicesPortal.Hostel.Models.Notification;

namespace CampusServicesPortal.Hostel.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationModel>> GetAllAsync();

        Task<NotificationModel?> GetByIdAsync(int id);

        Task<IEnumerable<NotificationModel>> GetByStudentIdAsync(int studentId);

        Task<NotificationModel> CreateAsync(CreateNotificationDto dto);

        Task<NotificationModel?> UpdateAsync(int id, UpdateNotificationDto dto);

        Task<bool> DeleteAsync(int id);
    }
}