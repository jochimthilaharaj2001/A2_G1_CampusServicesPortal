using CampusServicePortal.Data;
using CampusServicesPortal.Hostel.DTOs;
using Microsoft.EntityFrameworkCore;
using RoomModel = CampusServicesPortal.Hostel.Models.Room;

namespace CampusServicesPortal.Hostel.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoomModel>> GetAllAsync()
        {
            return await _context.Rooms
                .Include(r => r.Hostel)
                .ToListAsync();
        }

        public async Task<RoomModel?> GetByIdAsync(int id)
        {
            return await _context.Rooms
                .Include(r => r.Hostel)
                .FirstOrDefaultAsync(r => r.RoomId == id);
        }

        public async Task<RoomModel> CreateAsync(CreateRoomDto dto)
        {
            var room = new RoomModel
            {
                HostelId = dto.HostelId,
                RoomNumber = dto.RoomNumber,
                Capacity = dto.Capacity,
                CurrentOccupancy = 0,
                RoomType = dto.RoomType,
                IsActive = dto.IsActive
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return room;
        }

        public async Task<RoomModel?> UpdateAsync(int id, UpdateRoomDto dto)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return null;

            room.HostelId = dto.HostelId;
            room.RoomNumber = dto.RoomNumber;
            room.Capacity = dto.Capacity;
            // Occupancy is maintained only by the approved room-assignment workflow.
            room.RoomType = dto.RoomType;
            room.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return room;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return false;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RoomNumberExistsAsync(int hostelId, string roomNumber)
        {
            return await _context.Rooms.AnyAsync(r =>
                r.HostelId == hostelId &&
                r.RoomNumber == roomNumber);
        }

        public async Task<bool> RoomNumberExistsAsync(int hostelId, string roomNumber, int excludeRoomId)
        {
            return await _context.Rooms.AnyAsync(r =>
                r.HostelId == hostelId &&
                r.RoomNumber == roomNumber &&
                r.RoomId != excludeRoomId);
        }

        public async Task<bool> HostelExistsAsync(int hostelId)
        {
            return await _context.Hostels.AnyAsync(h => h.HostelId == hostelId);
        }
    }
}