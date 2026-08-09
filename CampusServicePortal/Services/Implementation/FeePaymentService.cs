using CampusServicePortal.Data;
using CampusServicePortal.DTOs.Fees;
using CampusServicePortal.Models;
using CampusServicePortal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Services.Implementation;

public class FeePaymentService(ApplicationDbContext context) : IFeePaymentService
{
    public async Task<IReadOnlyList<FeeTypeDto>> GetFeeTypesAsync(bool includeInactive = false) =>
        await context.FeeTypes.AsNoTracking().Where(f => includeInactive || f.IsActive).OrderBy(f => f.Name)
            .Select(f => new FeeTypeDto { FeeTypeId = f.FeeTypeId, Name = f.Name, Description = f.Description, IsActive = f.IsActive }).ToListAsync();

    public async Task<FeeTypeDto> SaveFeeTypeAsync(int? id, SaveFeeTypeDto dto)
    {
        var name = dto.Name.Trim();
        if (await context.FeeTypes.AnyAsync(f => f.Name == name && f.FeeTypeId != id)) throw new InvalidOperationException("A fee type with this name already exists.");
        FeeType type;
        if (id is null) { type = new FeeType { Name = name, Description = dto.Description?.Trim(), IsActive = dto.IsActive }; context.FeeTypes.Add(type); }
        else { type = await context.FeeTypes.FindAsync(id.Value) ?? throw new KeyNotFoundException("Fee type not found."); type.Name = name; type.Description = dto.Description?.Trim(); type.IsActive = dto.IsActive; }
        await context.SaveChangesAsync();
        return new FeeTypeDto { FeeTypeId = type.FeeTypeId, Name = type.Name, Description = type.Description, IsActive = type.IsActive };
    }

    public async Task<IReadOnlyList<FeePaymentDto>> GetForUserAsync(int userId) => await Query().Where(f => f.UserId == userId).OrderByDescending(f => f.AssignedAt).ToListAsync();
    public async Task<IReadOnlyList<FeePaymentDto>> GetAllAsync(string? status, int? feeTypeId)
    {
        var query = Query();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(f => f.Status == status);
        if (feeTypeId.HasValue) query = query.Where(f => f.FeeTypeId == feeTypeId.Value);
        return await query.OrderByDescending(f => f.AssignedAt).ToListAsync();
    }

    public async Task<int> AssignAsync(AssignFeeDto dto)
    {
        if (dto.StudentId is null && string.IsNullOrWhiteSpace(dto.StudentIndexNumber) && string.IsNullOrWhiteSpace(dto.Faculty)) throw new InvalidOperationException("Enter a student index number or select a faculty for bulk assignment.");
        var feeType = await context.FeeTypes.FindAsync(dto.FeeTypeId);
        if (feeType is null || !feeType.IsActive) throw new InvalidOperationException("Select a valid active fee type.");
        var students = context.Students.Include(s => s.User).Where(s => s.IsActive && s.User!.IsActive);
        if (!string.IsNullOrWhiteSpace(dto.StudentIndexNumber)) students = students.Where(s => s.IndexNumber == dto.StudentIndexNumber.Trim());
        else if (dto.StudentId.HasValue) students = students.Where(s => s.StudentId == dto.StudentId.Value);
        else students = students.Where(s => s.Faculty == dto.Faculty!.Trim());
        var targets = await students.ToListAsync();
        if (!targets.Any()) throw new InvalidOperationException("No active students match this assignment.");
        var period = dto.BillingPeriod.Trim();
        var userIds = targets.Select(s => s.UserId).ToList();
        var alreadyAssigned = await context.FeePayments.Where(f => userIds.Contains(f.UserId) && f.FeeTypeId == dto.FeeTypeId && f.BillingPeriod == period && f.Status == "Outstanding").Select(f => f.UserId).ToListAsync();
        foreach (var student in targets.Where(s => !alreadyAssigned.Contains(s.UserId))) context.FeePayments.Add(new FeePayment { UserId = student.UserId, FeeTypeId = dto.FeeTypeId, BillingPeriod = period, Amount = dto.Amount, Notes = dto.Notes?.Trim() });
        await context.SaveChangesAsync();
        return targets.Count - alreadyAssigned.Count;
    }

    public async Task<FeePaymentDto> UpdateAsync(int id, UpdateFeeDto dto)
    {
        var payment = await EditablePaymentAsync(id);
        payment.BillingPeriod = dto.BillingPeriod.Trim(); payment.Amount = dto.Amount; payment.Notes = dto.Notes?.Trim();
        await context.SaveChangesAsync(); return await GetByIdAsync(id);
    }
    public async Task CancelAsync(int id)
    {
        var payment = await EditablePaymentAsync(id); payment.Status = "Cancelled"; payment.CancelledAt = DateTime.UtcNow; await context.SaveChangesAsync();
    }
    public async Task<FeePaymentDto> PayAsync(int id, int userId, bool isAdmin)
    {
        var payment = await context.FeePayments.FindAsync(id) ?? throw new KeyNotFoundException("Fee payment not found.");
        if (!isAdmin && payment.UserId != userId) throw new UnauthorizedAccessException();
        if (payment.Status == "Paid") throw new InvalidOperationException("This fee has already been paid.");
        if (payment.Status != "Outstanding") throw new InvalidOperationException("Only outstanding fees can be paid.");
        payment.Status = "Paid"; payment.PaidAt = DateTime.UtcNow; payment.ReceiptNumber = $"CSP-{DateTime.UtcNow:yyyyMMdd}-{payment.FeePaymentId:D6}";
        await context.SaveChangesAsync(); return await GetByIdAsync(id);
    }
    public async Task<ReceiptDto> GetReceiptAsync(int id, int userId, bool isAdmin)
    {
        var payment = await GetByIdAsync(id);
        if (!isAdmin && payment.UserId != userId) throw new UnauthorizedAccessException();
        if (payment.Status != "Paid") throw new InvalidOperationException("A receipt is available only after payment.");
        return new ReceiptDto { FeePaymentId = payment.FeePaymentId, UserId = payment.UserId, StudentId = payment.StudentId, StudentName = payment.StudentName, IndexNumber = payment.IndexNumber, FeeTypeId = payment.FeeTypeId, FeeTypeName = payment.FeeTypeName, BillingPeriod = payment.BillingPeriod, Amount = payment.Amount, Status = payment.Status, Notes = payment.Notes, ReceiptNumber = payment.ReceiptNumber, AssignedAt = payment.AssignedAt, PaidAt = payment.PaidAt, GeneratedAt = DateTime.UtcNow };
    }
    private async Task<FeePayment> EditablePaymentAsync(int id)
    {
        var payment = await context.FeePayments.FindAsync(id) ?? throw new KeyNotFoundException("Fee payment not found.");
        if (payment.Status == "Paid") throw new InvalidOperationException("A paid fee assignment cannot be changed or cancelled.");
        if (payment.Status != "Outstanding") throw new InvalidOperationException("Only outstanding fee assignments can be changed.");
        return payment;
    }
    private IQueryable<FeePaymentDto> Query() => context.FeePayments.AsNoTracking().Include(f => f.User).ThenInclude(u => u!.Student).Include(f => f.FeeType).Select(f => new FeePaymentDto { FeePaymentId = f.FeePaymentId, UserId = f.UserId, StudentId = f.User!.Student != null ? f.User.Student.StudentId : 0, StudentName = f.User.FullName, IndexNumber = f.User.Student != null ? f.User.Student.IndexNumber : string.Empty, FeeTypeId = f.FeeTypeId, FeeTypeName = f.FeeType!.Name, BillingPeriod = f.BillingPeriod, Amount = f.Amount, Status = f.Status, Notes = f.Notes, ReceiptNumber = f.ReceiptNumber, AssignedAt = f.AssignedAt, PaidAt = f.PaidAt });
    private async Task<FeePaymentDto> GetByIdAsync(int id) => await Query().FirstAsync(f => f.FeePaymentId == id);
}
