using CampusServicePortal.Data;
using CampusServicePortal.DTOs.Certificates;
using CampusServicePortal.Models;
using CampusServicePortal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Services.Implementation;

public class CertificateRequestService(ApplicationDbContext context) : ICertificateRequestService
{
    public async Task<CertificateRequestDto> CreateAsync(int userId, CreateCertificateRequestDto dto)
    {
        var type = await context.CertificateTypes.FindAsync(dto.CertificateTypeId);
        if (type is null || !type.IsActive) throw new InvalidOperationException("Select a valid active certificate type.");
        if (await context.CertificateRequests.AnyAsync(r => r.UserId == userId && r.CertificateTypeId == dto.CertificateTypeId && r.Status == "Pending")) throw new InvalidOperationException("You already have a pending request for this certificate type.");
        var request = new CertificateRequest { UserId = userId, CertificateTypeId = dto.CertificateTypeId, Reason = dto.Reason.Trim() }; context.CertificateRequests.Add(request); await context.SaveChangesAsync(); return await Query().FirstAsync(r => r.CertificateRequestId == request.CertificateRequestId);
    }
    public async Task<IReadOnlyList<CertificateRequestDto>> GetForUserAsync(int userId) => await Query().Where(r => r.UserId == userId).OrderByDescending(r => r.RequestedAt).ToListAsync();
    public async Task<IReadOnlyList<CertificateRequestDto>> GetAllAsync(string? status) { var query = Query(); if (!string.IsNullOrWhiteSpace(status)) query = query.Where(r => r.Status == status); return await query.OrderByDescending(r => r.RequestedAt).ToListAsync(); }
    public async Task<CertificateRequestDto> UpdateStatusAsync(int id, UpdateCertificateRequestStatusDto dto) { var request = await context.CertificateRequests.FindAsync(id) ?? throw new KeyNotFoundException("Certificate request not found."); request.Status = dto.Status; request.AdminNote = dto.AdminNote?.Trim(); request.UpdatedAt = DateTime.UtcNow; await context.SaveChangesAsync(); return await Query().FirstAsync(r => r.CertificateRequestId == id); }
    private IQueryable<CertificateRequestDto> Query() => context.CertificateRequests.AsNoTracking().Include(r => r.User).ThenInclude(u => u!.Student).Include(r => r.CertificateType).Select(r => new CertificateRequestDto { CertificateRequestId = r.CertificateRequestId, UserId = r.UserId, StudentName = r.User!.FullName, IndexNumber = r.User.Student != null ? r.User.Student.IndexNumber : string.Empty, CertificateTypeId = r.CertificateTypeId, CertificateTypeName = r.CertificateType!.Name, Reason = r.Reason, Status = r.Status, AdminNote = r.AdminNote, RequestedAt = r.RequestedAt, UpdatedAt = r.UpdatedAt });
}
