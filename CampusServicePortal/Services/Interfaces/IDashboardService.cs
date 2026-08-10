using CampusServicePortal.DTOs.Dashboards;

namespace CampusServicePortal.Services.Interfaces;

public interface IDashboardService
{
    Task<StudentDashboardDto> GetStudentDashboardAsync(int userId);
    Task<AdminDashboardDto> GetAdminDashboardAsync();
}
