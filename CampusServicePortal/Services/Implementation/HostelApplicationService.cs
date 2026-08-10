using CampusServicesPortal.Hostel.DTOs;
using CampusServicesPortal.Hostel.Repositories;
using HostelApplicationModel = CampusServicesPortal.Hostel.Models.HostelApplication;

namespace CampusServicesPortal.Hostel.Services
{
    public class HostelApplicationService : IHostelApplicationService
    {
        private readonly IHostelApplicationRepository _repository;

        public HostelApplicationService(IHostelApplicationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<HostelApplicationModel>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<HostelApplicationModel?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<HostelApplicationModel> CreateAsync(CreateHostelApplicationDto dto)
        {
            if (!await _repository.StudentExistsAsync(dto.StudentId))
                throw new InvalidOperationException("Student does not exist.");

            if (!await _repository.HostelExistsAsync(dto.HostelId))
                throw new InvalidOperationException("Hostel does not exist.");

            if (await _repository.HasPendingApplicationAsync(dto.StudentId))
                throw new InvalidOperationException("Student already has a pending application.");

            return await _repository.CreateAsync(dto);
        }

        public async Task<HostelApplicationModel?> UpdateAsync(int id, UpdateHostelApplicationDto dto)
        {
            if (!await _repository.StudentExistsAsync(dto.StudentId))
                throw new InvalidOperationException("Student does not exist.");

            if (!await _repository.HostelExistsAsync(dto.HostelId))
                throw new InvalidOperationException("Hostel does not exist.");

            if (dto.RoomId.HasValue)
            {
                if (!await _repository.RoomExistsAsync(dto.RoomId.Value))
                    throw new InvalidOperationException("Assigned room does not exist.");
            }

            return await _repository.UpdateAsync(id, dto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}