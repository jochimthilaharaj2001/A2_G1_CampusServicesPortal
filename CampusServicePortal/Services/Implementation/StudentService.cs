using CampusServicePortal.DTOs.Students;
using CampusServicePortal.Models;
using CampusServicePortal.Repositories.Interfaces;
using CampusServicePortal.Services.Interfaces;

namespace CampusServicePortal.Services.Implementation
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<StudentProfileDto?> GetProfileAsync(int studentId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            return student == null ? null : MapToDto(student);
        }

        public async Task<StudentProfileDto?> GetProfileByUserIdAsync(int userId)
        {
            var student = await _studentRepository.GetByUserIdAsync(userId);
            return student == null ? null : MapToDto(student);
        }

        public async Task<StudentProfileDto> UpdateProfileAsync(int studentId, UpdateProfileDto dto)
        {
            var student = await _studentRepository.GetByIdAsync(studentId)
                ?? throw new KeyNotFoundException("Student not found.");

            // Update User fields
            if (student.User != null)
            {
                student.User.FullName = dto.FullName;
                student.User.PhoneNumber = dto.PhoneNumber;
            }

            // Update Student fields (IndexNumber is NOT touched — BRD rule: immutable)
            student.ContactNumber = dto.ContactNumber;
            student.Address = dto.Address;
            student.DegreeProgram = dto.DegreeProgram;

            await _studentRepository.UpdateAsync(student);
            await _studentRepository.SaveChangesAsync();

            return MapToDto(student);
        }

        public async Task<(IEnumerable<StudentProfileDto> Items, int TotalCount)> SearchStudentsAsync(
            string? search, string? faculty, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var (items, totalCount) = await _studentRepository.GetAllAsync(search, faculty, page, pageSize);
            var dtos = items.Select(MapToDto);
            return (dtos, totalCount);
        }

        public async Task<List<string>> CheckDeactivationBlockersAsync(int studentId)
        {
            var blockers = new List<string>();

            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
                throw new KeyNotFoundException("Student not found.");

            // NOTE: As other modules (Hostel, Lab, Events, etc.) are built,
            // inject their repositories here and add real checks.
            // For now, these are stubs that will be wired up in later modules.

            // Stub: Hostel active allocation check
            // if (await _hostelRepository.HasActiveAllocationAsync(studentId))
            //     blockers.Add("Student has an active hostel allocation (Approved or Room Assigned).");

            // Stub: Lab booking check
            // if (await _labRepository.HasUpcomingBookingsAsync(studentId))
            //     blockers.Add("Student has upcoming lab bookings.");

            // Stub: Event registration check
            // if (await _eventRepository.HasUpcomingRegistrationsAsync(studentId))
            //     blockers.Add("Student has upcoming event registrations.");

            // Stub: Certificate request check
            // if (await _certificateRepository.HasPendingRequestAsync(studentId))
            //     blockers.Add("Student has a pending certificate request.");

            // Stub: Unpaid fees check
            // if (await _feeRepository.HasUnpaidFeesAsync(studentId))
            //     blockers.Add("Student has unpaid fees or library fines.");

            return blockers;
        }

        public async Task DeactivateStudentAsync(int studentId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId)
                ?? throw new KeyNotFoundException("Student not found.");

            if (!student.IsActive)
                throw new InvalidOperationException("Student account is already deactivated.");

            var blockers = await CheckDeactivationBlockersAsync(studentId);
            if (blockers.Any())
                throw new InvalidOperationException(
                    "Cannot deactivate student: " + string.Join(" | ", blockers));

            student.IsActive = false;
            student.DeactivatedAt = DateTime.UtcNow;

            // Also deactivate the User record so login is blocked
            if (student.User != null)
            {
                student.User.IsActive = false;
            }

            await _studentRepository.UpdateAsync(student);
            await _studentRepository.SaveChangesAsync();
        }

        public async Task ReactivateStudentAsync(int studentId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId)
                ?? throw new KeyNotFoundException("Student not found.");

            if (student.IsActive)
                throw new InvalidOperationException("Student account is already active.");

            student.IsActive = true;
            student.DeactivatedAt = null;

            if (student.User != null)
            {
                student.User.IsActive = true;
            }

            await _studentRepository.UpdateAsync(student);
            await _studentRepository.SaveChangesAsync();
        }

        // ── Mapping helper ──────────────────────────────────────────────────────

        private static StudentProfileDto MapToDto(Student student)
        {
            return new StudentProfileDto
            {
                StudentId = student.StudentId,
                UserId = student.UserId,
                FullName = student.User?.FullName ?? string.Empty,
                Email = student.User?.Email ?? string.Empty,
                PhoneNumber = student.User?.PhoneNumber,
                EmailVerified = student.User?.EmailVerified ?? false,
                IsActive = student.IsActive,
                CreatedDate = student.User?.CreatedDate ?? DateTime.MinValue,
                IndexNumber = student.IndexNumber,
                Faculty = student.Faculty,
                DegreeProgram = student.DegreeProgram,
                EnrollmentYear = student.EnrollmentYear,
                ContactNumber = student.ContactNumber,
                Address = student.Address,
                DeactivatedAt = student.DeactivatedAt,
                Role = student.User?.Role?.RoleName ?? "Student"
            };
        }
    }
}
