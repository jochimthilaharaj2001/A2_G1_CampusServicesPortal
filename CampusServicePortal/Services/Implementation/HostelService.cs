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
        public async Task<IEnumerable<HostelModel>> GetAllAsync()
        {
            return await _hostelRepository.GetAllAsync();
        }
        public async Task<HostelModel?> GetByIdAsync(int id)
        {
            return await _hostelRepository.GetByIdAsync(id);
        }
        public async Task<HostelModel> CreateAsync(CreateHostelDto dto)
        {
            dto.HostelName = dto.HostelName.Trim();

            if (string.IsNullOrWhiteSpace(dto.HostelName))
            {
                throw new InvalidOperationException("Hostel name is required.");
            }
            dto.Gender = dto.Gender.Trim();

            if (string.IsNullOrWhiteSpace(dto.Gender))
            {
                throw new InvalidOperationException("Gender is required.");
            }

            dto.Gender = char.ToUpper(dto.Gender[0]) + dto.Gender.Substring(1).ToLower();

            if (dto.Gender != "Male" && dto.Gender != "Female")
            {
                throw new InvalidOperationException("Gender must be either 'Male' or 'Female'.");
            }

            if (await _hostelRepository.HostelNameExistsAsync(dto.HostelName))
            {
                throw new InvalidOperationException("A hostel with this name already exists.");
            }

            return await _hostelRepository.CreateAsync(dto);
        }
        public async Task<HostelModel?> UpdateAsync(int id, UpdateHostelDto dto)
        {
            // Trim hostel name
            dto.HostelName = dto.HostelName.Trim();

            if (string.IsNullOrWhiteSpace(dto.HostelName))
            {
                throw new InvalidOperationException("Hostel name is required.");
            }

            // Trim gender
            dto.Gender = dto.Gender.Trim();

            if (string.IsNullOrWhiteSpace(dto.Gender))
            {
                throw new InvalidOperationException("Gender is required.");
            }

            // Convert to proper case
            dto.Gender = char.ToUpper(dto.Gender[0]) +
                         dto.Gender.Substring(1).ToLower();

            if (dto.Gender != "Male" && dto.Gender != "Female")
            {
                throw new InvalidOperationException("Gender must be either 'Male' or 'Female'.");
            }
            if (await _hostelRepository.HostelNameExistsAsync(dto.HostelName, id))
            {
                throw new InvalidOperationException("A hostel with this name already exists.");
            }

            return await _hostelRepository.UpdateAsync(id, dto);
        }
        public async Task<bool> HostelNameExistsAsync(string hostelName)
        {
            return await _hostelRepository.HostelNameExistsAsync(hostelName);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            return await _hostelRepository.DeleteAsync(id);
        }
    }

}