using CampusServicesPortal.Hostel.DTOs;
using HostelApplicationModel = CampusServicesPortal.Hostel.Models.HostelApplication;

namespace CampusServicesPortal.Hostel.Services
{
    public interface IHostelApplicationService
    {
        Task<IEnumerable<HostelApplicationModel>> GetAllAsync();
        Task<IEnumerable<HostelApplicationModel>> GetByStudentIdAsync(int studentId);
        Task<HostelApplicationModel?> GetByIdAsync(int id);
        Task<HostelApplicationModel> CreateAsync(CreateHostelApplicationDto dto);
        Task<HostelApplicationModel?> UpdateAsync(int id, UpdateHostelApplicationDto dto);
        Task<HostelApplicationModel?> UpdateStatusAsync(int id, UpdateHostelApplicationStatusDto dto);
        Task<HostelApplicationModel?> AssignRoomAsync(int id, AssignHostelRoomDto dto);
        Task<bool> DeleteAsync(int id);
    }
}