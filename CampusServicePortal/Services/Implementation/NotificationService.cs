using CampusServicesPortal.Hostel.DTOs;
using CampusServicesPortal.Hostel.Repositories;
using NotificationModel = CampusServicesPortal.Hostel.Models.Notification;

namespace CampusServicesPortal.Hostel.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<NotificationModel>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<NotificationModel?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<NotificationModel>> GetByStudentIdAsync(int studentId)
        {
            if (!await _repository.StudentExistsAsync(studentId))
                throw new InvalidOperationException("Student does not exist.");

            return await _repository.GetByStudentIdAsync(studentId);
        }

        public async Task<NotificationModel> CreateAsync(CreateNotificationDto dto)
        {
            dto.Title = dto.Title.Trim();
            dto.Message = dto.Message.Trim();

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("Notification title is required.");

            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new InvalidOperationException("Notification message is required.");

            if (!await _repository.StudentExistsAsync(dto.StudentId))
                throw new InvalidOperationException("Student does not exist.");

            return await _repository.CreateAsync(dto);
        }

        public async Task<NotificationModel?> UpdateAsync(
            int id,
            UpdateNotificationDto dto)
        {
            dto.Title = dto.Title.Trim();
            dto.Message = dto.Message.Trim();

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("Notification title is required.");

            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new InvalidOperationException("Notification message is required.");

            return await _repository.UpdateAsync(id, dto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}