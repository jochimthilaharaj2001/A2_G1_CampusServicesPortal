using CampusServicePortal.DTOs.Students;
using CampusServicePortal.Models;
using CampusServicePortal.Repositories.Interfaces;
using CampusServicePortal.Services.Interfaces;

namespace CampusServicePortal.Services.Implementation
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IStudentMasterListRepository _masterListRepository;
        private readonly IFacultyRepository _facultyRepository;

        public StudentService(
            IStudentRepository studentRepository,
            IAuthRepository authRepository,
            IStudentMasterListRepository masterListRepository,
            IFacultyRepository facultyRepository)
        {
            _studentRepository = studentRepository;
            _authRepository = authRepository;
            _masterListRepository = masterListRepository;
            _facultyRepository = facultyRepository;
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

        public async Task<StudentProfileDto> CreateStudentByAdminAsync(AdminCreateStudentDto dto)
        {
            // 1. Check email uniqueness
            var existingUser = await _authRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("An account with this email address already exists.");

            // 2. Check index number uniqueness in current students
            var existingStudent = await _studentRepository.GetByIndexNumberAsync(dto.IndexNumber);
            if (existingStudent != null)
                throw new InvalidOperationException("A student with this index number already exists.");

            // 3. Check or create MasterList entry
            var masterRecord = await _masterListRepository.GetByIndexNumberAsync(dto.IndexNumber);
            if (masterRecord == null)
            {
                // Create it if it doesn't exist to maintain data integrity
                masterRecord = new StudentMasterList
                {
                    IndexNumber = dto.IndexNumber,
                    FullName = dto.FullName,
                    Faculty = dto.Faculty,
                    DegreeProgram = dto.DegreeProgram,
                    EnrollmentYear = dto.EnrollmentYear,
                    IsRegistered = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _masterListRepository.AddRangeAsync(new[] { masterRecord });
            }
            else
            {
                if (masterRecord.IsRegistered)
                    throw new InvalidOperationException("This index number is already registered.");
                await _masterListRepository.MarkAsRegisteredAsync(dto.IndexNumber);
            }

            // 4. Create User
            if (!string.IsNullOrWhiteSpace(dto.Password) && dto.Password.Length < 8)
                throw new InvalidOperationException("Password must be at least 8 characters.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                // If password is not provided by Admin, create a random default one
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(!string.IsNullOrWhiteSpace(dto.Password) ? dto.Password : Guid.NewGuid().ToString("N")),
                PhoneNumber = dto.PhoneNumber,
                RoleId = 2, // Student
                IsActive = true,
                EmailVerified = true, // Admin created accounts are verified by default
                CreatedDate = DateTime.UtcNow
            };

            await _authRepository.AddUserAsync(user);
            await _authRepository.SaveChangesAsync();

            // 5. Create Student
            var faculty = await ResolveFacultyAsync(dto.Faculty);
            var student = new Student
            {
                UserId = user.UserId,
                IndexNumber = dto.IndexNumber,
                FacultyId = faculty?.FacultyId,
                Faculty = faculty?.Name ?? dto.Faculty,
                DegreeProgram = dto.DegreeProgram,
                EnrollmentYear = dto.EnrollmentYear,
                ContactNumber = dto.ContactNumber,
                Address = dto.Address,
                IsActive = true
            };

            await _authRepository.AddStudentAsync(student);
            await _authRepository.SaveChangesAsync();
            await _masterListRepository.SaveChangesAsync();

            // Re-fetch to get complete object with role
            var newStudent = await _studentRepository.GetByIdAsync(student.StudentId);
            return MapToDto(newStudent!);
        }

        public async Task<StudentProfileDto> UpdateStudentByAdminAsync(int studentId, AdminUpdateStudentDto dto)
        {
            var student = await _studentRepository.GetByIdAsync(studentId)
                ?? throw new KeyNotFoundException("Student not found.");

            // Update User fields
            if (student.User != null)
            {
                student.User.FullName = dto.FullName;
                student.User.Email = dto.Email;
                student.User.PhoneNumber = dto.PhoneNumber;
                student.User.IsActive = dto.IsActive;
                student.User.EmailVerified = dto.EmailVerified;
            }

            // Update Student fields
            var faculty = await ResolveFacultyAsync(dto.Faculty);
            student.FacultyId = faculty?.FacultyId;
            student.Faculty = faculty?.Name ?? dto.Faculty;
            student.DegreeProgram = dto.DegreeProgram;
            student.EnrollmentYear = dto.EnrollmentYear;
            student.ContactNumber = dto.ContactNumber;
            student.Address = dto.Address;
            student.IsActive = dto.IsActive;
            
            if (!dto.IsActive && student.DeactivatedAt == null)
            {
                student.DeactivatedAt = DateTime.UtcNow;
            }
            else if (dto.IsActive && student.DeactivatedAt != null)
            {
                student.DeactivatedAt = null;
            }

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
                Faculty = student.FacultyNav?.Name ?? student.Faculty,
                DegreeProgram = student.DegreeProgram,
                EnrollmentYear = student.EnrollmentYear,
                ContactNumber = student.ContactNumber,
                Address = student.Address,
                DeactivatedAt = student.DeactivatedAt,
                Role = student.User?.Role?.RoleName ?? "Student",
                // Placeholder until Modules 2–8 wire real data into CheckDeactivationBlockers / profile.
                ActivitySummary = new StudentActivitySummaryDto()
            };
        }

        private async Task<Faculty?> ResolveFacultyAsync(string? facultyName)
        {
            if (string.IsNullOrWhiteSpace(facultyName))
                return null;

            var existing = await _facultyRepository.GetByNameAsync(facultyName.Trim());
            if (existing != null)
                return existing;

            var faculty = new Faculty
            {
                Name = facultyName.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _facultyRepository.AddAsync(faculty);
            await _facultyRepository.SaveChangesAsync();
            return faculty;
        }
    }
}
