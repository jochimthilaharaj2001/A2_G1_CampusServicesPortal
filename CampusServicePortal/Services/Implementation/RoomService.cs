using CampusServicesPortal.Hostel.DTOs;
using CampusServicesPortal.Hostel.Repositories;
using CampusServicePortal.Data;
using Microsoft.EntityFrameworkCore;
using RoomModel = CampusServicesPortal.Hostel.Models.Room;

namespace CampusServicesPortal.Hostel.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly ApplicationDbContext _context;

        public RoomService(IRoomRepository roomRepository, ApplicationDbContext context)
        {
            _roomRepository = roomRepository;
            _context = context;
        }

        public Task<IEnumerable<RoomModel>> GetAllAsync() => _roomRepository.GetAllAsync();
        public Task<RoomModel?> GetByIdAsync(int id) => _roomRepository.GetByIdAsync(id);

        public async Task<RoomModel> CreateAsync(CreateRoomDto dto)
        {
            ValidateRoom(dto);
            if (!await _roomRepository.HostelExistsAsync(dto.HostelId))
                throw new InvalidOperationException("Selected hostel does not exist.");
            if (await _roomRepository.RoomNumberExistsAsync(dto.HostelId, dto.RoomNumber))
                throw new InvalidOperationException("Room number already exists in this hostel.");

            return await _roomRepository.CreateAsync(dto);
        }

        public async Task<RoomModel?> UpdateAsync(int id, UpdateRoomDto dto)
        {
            ValidateRoom(dto);
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
                return null;

            var assignedOccupancy = await _context.HostelApplications.CountAsync(application =>
                application.RoomId == id && application.Status == "Room Assigned");
            if (assignedOccupancy != room.CurrentOccupancy)
                room.CurrentOccupancy = assignedOccupancy;

            if (dto.Capacity < assignedOccupancy)
                throw new InvalidOperationException("Capacity cannot be lower than current room occupancy.");
            if (dto.HostelId != room.HostelId && assignedOccupancy > 0)
                throw new InvalidOperationException("An occupied room cannot be moved to another hostel.");
            if (!await _roomRepository.HostelExistsAsync(dto.HostelId))
                throw new InvalidOperationException("Selected hostel does not exist.");
            if (await _roomRepository.RoomNumberExistsAsync(dto.HostelId, dto.RoomNumber, id))
                throw new InvalidOperationException("Room number already exists in this hostel.");

            return await _roomRepository.UpdateAsync(id, dto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
                return false;
            var assignedOccupancy = await _context.HostelApplications.CountAsync(application =>
                application.RoomId == id && application.Status == "Room Assigned");
            if (assignedOccupancy > 0 || room.CurrentOccupancy > 0)
                throw new InvalidOperationException(
                    "This room is occupied. Deactivate it instead of deleting it.");

            return await _roomRepository.DeleteAsync(id);
        }

        private static void ValidateRoom(CreateRoomDto dto)
        {
            dto.RoomNumber = dto.RoomNumber.Trim();
            if (string.IsNullOrWhiteSpace(dto.RoomNumber))
                throw new InvalidOperationException("Room number is required.");
            if (dto.Capacity <= 0)
                throw new InvalidOperationException("Capacity must be greater than zero.");
        }
        private static void ValidateRoom(UpdateRoomDto dto)
        {
            dto.RoomNumber = (dto.RoomNumber ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(dto.RoomNumber))
                throw new InvalidOperationException("Room number is required.");
            if (dto.Capacity <= 0)
                throw new InvalidOperationException("Capacity must be greater than zero.");
        }
    }
}