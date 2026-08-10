using CampusServicesPortal.Hostel.DTOs;
using CampusServicesPortal.Hostel.Repositories;
using HostelModel = CampusServicesPortal.Hostel.Models.Hostel;

namespace CampusServicesPortal.Hostel.Services
{
    public class HostelService : IHostelService
    {
        private readonly IHostelRepository _hostelRepository;

        public HostelService(IHostelRepository hostelRepository)
        {
            _hostelRepository = hostelRepository;
        }

        public Task<IEnumerable<HostelModel>> GetAllAsync() => _hostelRepository.GetAllAsync();
        public Task<HostelModel?> GetByIdAsync(int id) => _hostelRepository.GetByIdAsync(id);

        public async Task<HostelModel> CreateAsync(CreateHostelDto dto)
        {
            Normalize(dto);
            if (await _hostelRepository.HostelNameExistsAsync(dto.HostelName))
                throw new InvalidOperationException("A hostel with this name already exists.");

            return await _hostelRepository.CreateAsync(dto);
        }

        public async Task<HostelModel?> UpdateAsync(int id, UpdateHostelDto dto)
        {
            Normalize(dto);
            if (await _hostelRepository.HostelNameExistsAsync(dto.HostelName, id))
                throw new InvalidOperationException("A hostel with this name already exists.");

            return await _hostelRepository.UpdateAsync(id, dto);
        }

        public Task<bool> HostelNameExistsAsync(string hostelName) =>
            _hostelRepository.HostelNameExistsAsync(hostelName);

        public async Task<bool> DeleteAsync(int id)
        {
            if (await _hostelRepository.IsInUseAsync(id))
                throw new InvalidOperationException(
                    "This hostel has rooms or applications. Deactivate it instead of deleting it.");

            return await _hostelRepository.DeleteAsync(id);
        }

        private static void Normalize(CreateHostelDto dto)
        {
            dto.HostelName = dto.HostelName.Trim();
            dto.Gender = dto.Gender.Trim();

            if (string.IsNullOrWhiteSpace(dto.HostelName))
                throw new InvalidOperationException("Hostel name is required.");
            if (string.IsNullOrWhiteSpace(dto.Gender))
                throw new InvalidOperationException("Gender is required.");

            dto.Gender = char.ToUpper(dto.Gender[0]) + dto.Gender[1..].ToLower();
            if (dto.Gender is not ("Male" or "Female"))
                throw new InvalidOperationException("Gender must be either 'Male' or 'Female'.");
        }
        private static void Normalize(UpdateHostelDto dto)
        {
            dto.HostelName = (dto.HostelName ?? string.Empty).Trim();
            dto.Gender = (dto.Gender ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(dto.HostelName))
                throw new InvalidOperationException("Hostel name is required.");
            if (string.IsNullOrWhiteSpace(dto.Gender))
                throw new InvalidOperationException("Gender is required.");

            dto.Gender = char.ToUpper(dto.Gender[0]) + dto.Gender[1..].ToLower();
            if (dto.Gender is not ("Male" or "Female"))
                throw new InvalidOperationException("Gender must be either 'Male' or 'Female'.");
        }
    }
}