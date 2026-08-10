using CampusServicePortal.Data;
using CampusServicesPortal.Hostel.DTOs;
using Microsoft.EntityFrameworkCore;
using HostelApplicationModel = CampusServicesPortal.Hostel.Models.HostelApplication;

namespace CampusServicesPortal.Hostel.Repositories
{
    public class HostelApplicationRepository : IHostelApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public HostelApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HostelApplicationModel>> GetAllAsync()
        {
            return await _context.HostelApplications
                .Include(a => a.Student)
                .Include(a => a.Hostel)
                .Include(a => a.Room)
                .ToListAsync();
        }

        public async Task<HostelApplicationModel?> GetByIdAsync(int id)
        {
            return await _context.HostelApplications
                .Include(a => a.Student)
                .Include(a => a.Hostel)
                .Include(a => a.Room)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);
        }

        public async Task<HostelApplicationModel> CreateAsync(CreateHostelApplicationDto dto)
        {
            var application = new HostelApplicationModel
            {
                StudentId = dto.StudentId,
                HostelId = dto.HostelId,
                Semester = dto.Semester,
                SpecialRequirements = dto.SpecialRequirements,
                Status = "Pending",
                AppliedDate = DateTime.UtcNow
            };

            _context.HostelApplications.Add(application);
            await _context.SaveChangesAsync();

            return application;
        }

        public async Task<HostelApplicationModel?> UpdateAsync(int id, UpdateHostelApplicationDto dto)
        {
            var application = await _context.HostelApplications.FindAsync(id);

            if (application == null)
                return null;

            application.StudentId = dto.StudentId;
            application.HostelId = dto.HostelId;
            application.RoomId = dto.RoomId;
            application.Semester = dto.Semester;
            application.SpecialRequirements = dto.SpecialRequirements;
            application.Status = dto.Status;

            await _context.SaveChangesAsync();

            return application;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var application = await _context.HostelApplications.FindAsync(id);

            if (application == null)
                return false;

            _context.HostelApplications.Remove(application);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> StudentExistsAsync(int studentId)
        {
            return await _context.Students.AnyAsync(s => s.StudentId == studentId);
        }

        public async Task<bool> HostelExistsAsync(int hostelId)
        {
            return await _context.Hostels.AnyAsync(h => h.HostelId == hostelId);
        }

        public async Task<bool> RoomExistsAsync(int roomId)
        {
            return await _context.Rooms.AnyAsync(r => r.RoomId == roomId);
        }

        public async Task<bool> HasPendingApplicationAsync(int studentId)
        {
            return await _context.HostelApplications.AnyAsync(a =>
                a.StudentId == studentId &&
                a.Status == "Pending");
        }
    }
}