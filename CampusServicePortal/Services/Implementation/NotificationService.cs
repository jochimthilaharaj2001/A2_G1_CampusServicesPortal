using CampusServicesPortal.Hostel.Repositories;
using CampusServicesPortal.Hostel.DTOs;
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

        public Task<IEnumerable<NotificationModel>> GetAllAsync() =>
            _repository.GetAllAsync();

        public Task<NotificationModel?> GetByIdAsync(int id) =>
            _repository.GetByIdAsync(id);

        public async Task<NotificationModel> CreateAsync(CreateNotificationDto dto)
        {
            if (!await _repository.StudentExistsAsync(dto.StudentId))
                throw new InvalidOperationException("Student does not exist.");

            dto.Title = (dto.Title ?? string.Empty).Trim();
            dto.Message = (dto.Message ?? string.Empty).Trim();
            dto.Type = (dto.Type ?? "System").Trim();

            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Message))
                throw new InvalidOperationException("Title and message are required.");

            return await _repository.CreateAsync(dto);
        }

        public async Task<NotificationModel?> UpdateAsync(int id, UpdateNotificationDto dto)
        {
            dto.Title = (dto.Title ?? string.Empty).Trim();
            dto.Message = (dto.Message ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Message))
                throw new InvalidOperationException("Title and message are required.");

            return await _repository.UpdateAsync(id, dto);
        }

        public Task<bool> DeleteAsync(int id) =>
            _repository.DeleteAsync(id);

        public async Task<IEnumerable<NotificationModel>> GetByStudentIdAsync(int studentId)
        {
            if (!await _repository.StudentExistsAsync(studentId))
                throw new InvalidOperationException("Student does not exist.");

            return await _repository.GetByStudentIdAsync(studentId);
        }

        public Task<NotificationModel?> MarkAsReadAsync(int id, int studentId) =>
            _repository.MarkAsReadAsync(id, studentId);
    }
}