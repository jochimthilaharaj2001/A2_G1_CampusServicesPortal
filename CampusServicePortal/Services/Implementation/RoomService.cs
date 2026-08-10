using CampusServicesPortal.Hostel.DTOs;
using CampusServicesPortal.Hostel.Repositories;
using RoomModel = CampusServicesPortal.Hostel.Models.Room;

namespace CampusServicesPortal.Hostel.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;

        public RoomService(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<IEnumerable<RoomModel>> GetAllAsync()
        {
            return await _roomRepository.GetAllAsync();
        }

        public async Task<RoomModel?> GetByIdAsync(int id)
        {
            return await _roomRepository.GetByIdAsync(id);
        }

        public async Task<RoomModel> CreateAsync(CreateRoomDto dto)
        {
            dto.RoomNumber = dto.RoomNumber.Trim();

            if (string.IsNullOrWhiteSpace(dto.RoomNumber))
                throw new InvalidOperationException("Room number is required.");

            if (dto.Capacity <= 0)
                throw new InvalidOperationException("Capacity must be greater than zero.");

            if (!await _roomRepository.HostelExistsAsync(dto.HostelId))
                throw new InvalidOperationException("Selected hostel does not exist.");

            if (await _roomRepository.RoomNumberExistsAsync(dto.HostelId, dto.RoomNumber))
                throw new InvalidOperationException("Room number already exists in this hostel.");

            return await _roomRepository.CreateAsync(dto);
        }

        public async Task<RoomModel?> UpdateAsync(int id, UpdateRoomDto dto)
        {
            dto.RoomNumber = dto.RoomNumber.Trim();

            if (string.IsNullOrWhiteSpace(dto.RoomNumber))
                throw new InvalidOperationException("Room number is required.");

            if (dto.Capacity <= 0)
                throw new InvalidOperationException("Capacity must be greater than zero.");

            if (dto.CurrentOccupancy > dto.Capacity)
                throw new InvalidOperationException("Current occupancy cannot exceed room capacity.");

            if (!await _roomRepository.HostelExistsAsync(dto.HostelId))
                throw new InvalidOperationException("Selected hostel does not exist.");

            if (await _roomRepository.RoomNumberExistsAsync(dto.HostelId, dto.RoomNumber, id))
                throw new InvalidOperationException("Room number already exists in this hostel.");

            return await _roomRepository.UpdateAsync(id, dto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _roomRepository.DeleteAsync(id);
        }
    }
}