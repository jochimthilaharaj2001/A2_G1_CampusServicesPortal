using CampusServicePortal.Data;
using CampusServicePortal.DTOs.Complaints;
using CampusServicePortal.Models;
using CampusServicePortal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Services.Implementation;

public class ComplaintService(ApplicationDbContext context) : IComplaintService
{
    public async Task<IReadOnlyList<ComplaintCategoryDto>> GetCategoriesAsync(bool includeInactive = false) =>
        await context.ComplaintCategories.AsNoTracking()
            .Where(c => includeInactive || c.IsActive).OrderBy(c => c.Name)
            .Select(c => ToCategoryDto(c)).ToListAsync();

    public async Task<ComplaintCategoryDto> SaveCategoryAsync(int? id, SaveComplaintCategoryDto dto)
    {
        var name = dto.Name.Trim();
        if (await context.ComplaintCategories.AnyAsync(c => c.Name == name && c.ComplaintCategoryId != id))
            throw new InvalidOperationException("A complaint category with this name already exists.");

        ComplaintCategory category;
        if (id is null)
        {
            category = new ComplaintCategory { Name = name, Description = dto.Description?.Trim(), IsActive = dto.IsActive };
            context.ComplaintCategories.Add(category);
        }
        else
        {
            category = await context.ComplaintCategories.FindAsync(id.Value)
                ?? throw new KeyNotFoundException("Complaint category not found.");
            category.Name = name;
            category.Description = dto.Description?.Trim();
            category.IsActive = dto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return ToCategoryDto(category);
    }

    public async Task DeactivateCategoryAsync(int id)
    {
        var category = await context.ComplaintCategories.FindAsync(id)
            ?? throw new KeyNotFoundException("Complaint category not found.");
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<ComplaintDto> CreateAsync(int userId, CreateComplaintDto dto)
    {
        var category = await context.ComplaintCategories.FindAsync(dto.ComplaintCategoryId);
        if (category is null || !category.IsActive)
            throw new InvalidOperationException("Select a valid active complaint category.");

        var complaint = new Complaint { UserId = userId, ComplaintCategoryId = category.ComplaintCategoryId, Description = dto.Description.Trim() };
        context.Complaints.Add(complaint);
        await context.SaveChangesAsync();
        return await GetByIdAsync(complaint.ComplaintId);
    }

    public async Task<IReadOnlyList<ComplaintDto>> GetForUserAsync(int userId) =>
        await Query().Where(c => c.UserId == userId).OrderByDescending(c => c.CreatedAt).ToListAsync();

    public async Task<IReadOnlyList<ComplaintDto>> GetAllAsync(string? status)
    {
        var query = Query();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(c => c.Status == status);
        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<ComplaintDto> UpdateStatusAsync(int id, UpdateComplaintStatusDto dto)
    {
        var complaint = await context.Complaints.FindAsync(id)
            ?? throw new KeyNotFoundException("Complaint not found.");
        complaint.Status = dto.Status;
        complaint.ResolutionNote = dto.ResolutionNote?.Trim();
        complaint.UpdatedAt = DateTime.UtcNow;
        complaint.ResolvedAt = dto.Status == "Resolved" ? DateTime.UtcNow : null;
        await context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    private IQueryable<ComplaintDto> Query() => context.Complaints.AsNoTracking().Include(c => c.User).ThenInclude(u => u!.Student).Include(c => c.Category)
        .Select(c => new ComplaintDto { ComplaintId = c.ComplaintId, UserId = c.UserId, StudentName = c.User!.FullName, IndexNumber = c.User.Student != null ? c.User.Student.IndexNumber : string.Empty, ComplaintCategoryId = c.ComplaintCategoryId, CategoryName = c.Category!.Name, Description = c.Description, Status = c.Status, ResolutionNote = c.ResolutionNote, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt, ResolvedAt = c.ResolvedAt });
    private async Task<ComplaintDto> GetByIdAsync(int id) => await Query().FirstAsync(c => c.ComplaintId == id);
    private static ComplaintCategoryDto ToCategoryDto(ComplaintCategory c) => new() { ComplaintCategoryId = c.ComplaintCategoryId, Name = c.Name, Description = c.Description, IsActive = c.IsActive };
}
