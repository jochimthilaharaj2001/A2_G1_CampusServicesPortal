using CampusServicePortal.Data;
using CampusServicesPortal.Hostel.DTOs;
using CampusServicesPortal.Hostel.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Data;
using HostelApplicationModel = CampusServicesPortal.Hostel.Models.HostelApplication;

namespace CampusServicesPortal.Hostel.Services
{
    public class HostelApplicationService : IHostelApplicationService
    {
        private const string Pending = "Pending";
        private const string Approved = "Approved";
        private const string Rejected = "Rejected";
        private const string RoomAssigned = "Room Assigned";

        private readonly IHostelApplicationRepository _repository;
        private readonly ApplicationDbContext _context;
        private readonly INotificationQueue _notifications;

        public HostelApplicationService(
            IHostelApplicationRepository repository,
            ApplicationDbContext context,
            INotificationQueue notifications)
        {
            _repository = repository;
            _context = context;
            _notifications = notifications;
        }

        public Task<IEnumerable<HostelApplicationModel>> GetAllAsync() => _repository.GetAllAsync();

        public async Task<IEnumerable<HostelApplicationModel>> GetByStudentIdAsync(int studentId)
        {
            return await _context.HostelApplications
                .AsNoTracking()
                .Include(application => application.Hostel)
                .Include(application => application.Room)
                .Where(application => application.StudentId == studentId)
                .OrderByDescending(application => application.AppliedDate)
                .ToListAsync();
        }

        public Task<HostelApplicationModel?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

        public async Task<HostelApplicationModel> CreateAsync(CreateHostelApplicationDto dto)
        {
            dto.Semester = (dto.Semester ?? string.Empty).Trim();
            dto.SpecialRequirements = dto.SpecialRequirements?.Trim();

            if (string.IsNullOrWhiteSpace(dto.Semester))
                throw new InvalidOperationException("Semester is required.");

            if (!await _repository.StudentExistsAsync(dto.StudentId))
                throw new InvalidOperationException("Student does not exist.");

            var hostelIsActive = await _context.Hostels.AnyAsync(hostel =>
                hostel.HostelId == dto.HostelId && hostel.IsActive);

            if (!hostelIsActive)
                throw new InvalidOperationException("Select an active hostel.");

            if (await _repository.HasPendingApplicationAsync(dto.StudentId))
                throw new InvalidOperationException("Student already has a pending application.");

            if (await _context.HostelApplications.AnyAsync(application => application.StudentId == dto.StudentId && (application.Status == Approved || application.Status == RoomAssigned)))
                throw new InvalidOperationException("Student already has an active hostel allocation.");

            return await _repository.CreateAsync(dto);
        }

        public async Task<HostelApplicationModel?> UpdateAsync(int id, UpdateHostelApplicationDto dto)
        {
            var application = await _context.HostelApplications
                .FirstOrDefaultAsync(item => item.ApplicationId == id);

            if (application == null)
                return null;

            if (application.Status != Pending)
                throw new InvalidOperationException("Only pending applications can be edited.");

            if (application.StudentId != dto.StudentId)
                throw new InvalidOperationException("The applicant cannot be changed.");

            if (dto.RoomId.HasValue || !string.Equals(dto.Status, Pending, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Use the dedicated status or room-assignment action.");

            dto.Semester = (dto.Semester ?? string.Empty).Trim();
            dto.SpecialRequirements = dto.SpecialRequirements?.Trim();
            if (string.IsNullOrWhiteSpace(dto.Semester))
                throw new InvalidOperationException("Semester is required.");

            var hostelIsActive = await _context.Hostels.AnyAsync(hostel =>
                hostel.HostelId == dto.HostelId && hostel.IsActive);

            if (!hostelIsActive)
                throw new InvalidOperationException("Select an active hostel.");

            application.HostelId = dto.HostelId;
            application.Semester = dto.Semester;
            application.SpecialRequirements = dto.SpecialRequirements?.Trim();

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<HostelApplicationModel?> UpdateStatusAsync(
            int id,
            UpdateHostelApplicationStatusDto dto)
        {
            var application = await _context.HostelApplications
                .FirstOrDefaultAsync(item => item.ApplicationId == id);

            if (application == null)
                return null;

            if (application.Status != Pending)
                throw new InvalidOperationException("Only pending applications can be approved or rejected.");

            var status = dto.Status.Trim();
            if (status is not Approved and not Rejected)
                throw new InvalidOperationException("Status must be Approved or Rejected.");

            application.Status = status;
            await _notifications.QueueForStudentAsync(
                application.StudentId,
                status == Approved ? "HostelApproved" : "HostelRejected",
                "Hostel application update",
                status == Approved
                    ? "Your hostel application has been approved. Room assignment will follow."
                    : "Your hostel application has been rejected.");

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<HostelApplicationModel?> AssignRoomAsync(int id, AssignHostelRoomDto dto)
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var application = await _context.HostelApplications
                .FirstOrDefaultAsync(item => item.ApplicationId == id);

            if (application == null)
                return null;

            if (application.Status != Approved)
                throw new InvalidOperationException("Approve the application before assigning a room.");

            var room = await _context.Rooms
                .Include(item => item.Hostel)
                .FirstOrDefaultAsync(item => item.RoomId == dto.RoomId);

            if (room == null || !room.IsActive || room.Hostel is null || !room.Hostel.IsActive)
                throw new InvalidOperationException("Select an active room.");

            if (room.HostelId != application.HostelId)
                throw new InvalidOperationException("The selected room does not belong to the requested hostel.");

            if (room.CurrentOccupancy >= room.Capacity)
                throw new InvalidOperationException("The selected room is already at full capacity.");

            application.RoomId = room.RoomId;
            application.Status = RoomAssigned;
            room.CurrentOccupancy++;

            await _notifications.QueueForStudentAsync(
                application.StudentId,
                "HostelRoomAssigned",
                "Hostel room assigned",
                $"Room {room.RoomNumber} has been assigned to your hostel application.");

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var application = await _context.HostelApplications
                .FirstOrDefaultAsync(item => item.ApplicationId == id);

            if (application == null)
                return false;

            if (application.Status != Pending)
                throw new InvalidOperationException("Only pending applications can be deleted.");

            _context.HostelApplications.Remove(application);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}