using CampusServicesPortal.Hostel.DTOs;
using HostelApplicationModel = CampusServicesPortal.Hostel.Models.HostelApplication;

namespace CampusServicesPortal.Hostel.Repositories
{
    public interface IHostelApplicationRepository
    {
        Task<IEnumerable<HostelApplicationModel>> GetAllAsync();

        Task<HostelApplicationModel?> GetByIdAsync(int id);

        Task<HostelApplicationModel> CreateAsync(CreateHostelApplicationDto dto);

        Task<HostelApplicationModel?> UpdateAsync(int id, UpdateHostelApplicationDto dto);

        Task<bool> DeleteAsync(int id);

        Task<bool> StudentExistsAsync(int studentId);

        Task<bool> HostelExistsAsync(int hostelId);

        Task<bool> RoomExistsAsync(int roomId);

        Task<bool> HasPendingApplicationAsync(int studentId);
    }
}