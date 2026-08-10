using CampusServicePortal.Data;
using CampusServicesPortal.Hostel.DTOs;
using Microsoft.EntityFrameworkCore;
using HostelModel = CampusServicesPortal.Hostel.Models.Hostel;

namespace CampusServicesPortal.Hostel.Repositories
{
    public class HostelRepository : IHostelRepository
    {
        private readonly ApplicationDbContext _context;

        public HostelRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<IEnumerable<HostelModel>> GetAllAsync() =>
            _context.Hostels.ToListAsync().ContinueWith(task => (IEnumerable<HostelModel>)task.Result);

        public Task<HostelModel?> GetByIdAsync(int id) =>
            _context.Hostels.FindAsync(id).AsTask();

        public async Task<HostelModel> CreateAsync(CreateHostelDto dto)
        {
            var hostel = new HostelModel
            {
                HostelName = dto.HostelName,
                Gender = dto.Gender,
                Location = dto.Location,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            _context.Hostels.Add(hostel);
            await _context.SaveChangesAsync();
            return hostel;
        }

        public async Task<HostelModel?> UpdateAsync(int id, UpdateHostelDto dto)
        {
            var hostel = await _context.Hostels.FindAsync(id);
            if (hostel == null)
                return null;

            hostel.HostelName = dto.HostelName;
            hostel.Gender = dto.Gender;
            hostel.Location = dto.Location;
            hostel.Description = dto.Description;
            hostel.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return hostel;
        }

        public Task<bool> HostelNameExistsAsync(string hostelName) =>
            _context.Hostels.AnyAsync(hostel => hostel.HostelName == hostelName);

        public Task<bool> HostelNameExistsAsync(string hostelName, int excludeId) =>
            _context.Hostels.AnyAsync(hostel =>
                hostel.HostelName == hostelName && hostel.HostelId != excludeId);

        public async Task<bool> IsInUseAsync(int id)
        {
            return await _context.Rooms.AnyAsync(room => room.HostelId == id)
                || await _context.HostelApplications.AnyAsync(application =>
                    application.HostelId == id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var hostel = await _context.Hostels.FindAsync(id);
            if (hostel == null)
                return false;

            _context.Hostels.Remove(hostel);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}