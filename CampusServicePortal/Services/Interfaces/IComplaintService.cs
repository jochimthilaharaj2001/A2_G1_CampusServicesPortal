using CampusServicePortal.DTOs.Complaints;

namespace CampusServicePortal.Services.Interfaces;

public interface IComplaintService
{
    Task<IReadOnlyList<ComplaintCategoryDto>> GetCategoriesAsync(bool includeInactive = false);
    Task<ComplaintCategoryDto> SaveCategoryAsync(int? id, SaveComplaintCategoryDto dto);
    Task DeactivateCategoryAsync(int id);
    Task<ComplaintDto> CreateAsync(int userId, CreateComplaintDto dto);
    Task<IReadOnlyList<ComplaintDto>> GetForUserAsync(int userId);
    Task<IReadOnlyList<ComplaintDto>> GetAllAsync(string? status);
    Task<ComplaintDto> UpdateStatusAsync(int id, UpdateComplaintStatusDto dto);
}
