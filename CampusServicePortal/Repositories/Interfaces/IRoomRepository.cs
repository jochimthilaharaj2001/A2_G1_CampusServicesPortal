using CampusServicesPortal.Hostel.DTOs;
using RoomModel = CampusServicesPortal.Hostel.Models.Room;

namespace CampusServicesPortal.Hostel.Repositories
{
    public interface IRoomRepository
    {
        Task<IEnumerable<RoomModel>> GetAllAsync();

        Task<RoomModel?> GetByIdAsync(int id);

        Task<RoomModel> CreateAsync(CreateRoomDto dto);

        Task<RoomModel?> UpdateAsync(int id, UpdateRoomDto dto);

        Task<bool> DeleteAsync(int id);

        Task<bool> RoomNumberExistsAsync(int hostelId, string roomNumber);

        Task<bool> RoomNumberExistsAsync(int hostelId, string roomNumber, int excludeRoomId);

        Task<bool> HostelExistsAsync(int hostelId);
    }
}