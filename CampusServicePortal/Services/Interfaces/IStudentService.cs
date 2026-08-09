using CampusServicePortal.DTOs.Students;

namespace CampusServicePortal.Services.Interfaces
{
    public interface IStudentService
    {
        Task<StudentProfileDto?> GetProfileAsync(int studentId);
        Task<StudentProfileDto?> GetProfileByUserIdAsync(int userId);
        Task<StudentProfileDto> UpdateProfileAsync(int studentId, UpdateProfileDto dto);
        Task<(IEnumerable<StudentProfileDto> Items, int TotalCount)> SearchStudentsAsync(
            string? search, string? faculty, int page, int pageSize);
        Task DeactivateStudentAsync(int studentId);
        Task ReactivateStudentAsync(int studentId);
        Task<List<string>> CheckDeactivationBlockersAsync(int studentId);
        Task<StudentProfileDto> CreateStudentByAdminAsync(AdminCreateStudentDto dto);
        Task<StudentProfileDto> UpdateStudentByAdminAsync(int studentId, AdminUpdateStudentDto dto);
    }
}
