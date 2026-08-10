using CampusServicesPortal.Hostel.DTOs;
using RoomModel = CampusServicesPortal.Hostel.Models.Room;

namespace CampusServicesPortal.Hostel.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomModel>> GetAllAsync();

        Task<RoomModel?> GetByIdAsync(int id);

        Task<RoomModel> CreateAsync(CreateRoomDto dto);

        Task<RoomModel?> UpdateAsync(int id, UpdateRoomDto dto);

        Task<bool> DeleteAsync(int id);
    }
}