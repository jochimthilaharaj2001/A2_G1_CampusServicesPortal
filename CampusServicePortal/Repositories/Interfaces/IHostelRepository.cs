using CampusServicesPortal.Hostel.DTOs;
using HostelModel = CampusServicesPortal.Hostel.Models.Hostel;
namespace CampusServicesPortal.Hostel.Repositories
{
    public interface IHostelRepository
    {
        Task<IEnumerable<HostelModel>> GetAllAsync();

        Task<HostelModel?> GetByIdAsync(int id);

        Task<HostelModel> CreateAsync(CreateHostelDto dto);

        Task<HostelModel?> UpdateAsync(int id, UpdateHostelDto dto);

        Task<bool> HostelNameExistsAsync(string hostelName);

        Task<bool> HostelNameExistsAsync(string hostelName, int excludeId);
        Task<bool> DeleteAsync(int id);
    }
}