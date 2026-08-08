using CampusServicesPortal.Hostel.DTOs;
using HostelApplicationModel = CampusServicesPortal.Hostel.Models.HostelApplication;

namespace CampusServicesPortal.Hostel.Services
{
    public interface IHostelApplicationService
    {
        Task<IEnumerable<HostelApplicationModel>> GetAllAsync();

        Task<HostelApplicationModel?> GetByIdAsync(int id);

        Task<HostelApplicationModel> CreateAsync(CreateHostelApplicationDto dto);

        Task<HostelApplicationModel?> UpdateAsync(int id, UpdateHostelApplicationDto dto);

        Task<bool> DeleteAsync(int id);
    }
}