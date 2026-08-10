using CampusServicePortal.DTOs.Fees;

namespace CampusServicePortal.Services.Interfaces;

public interface IFeePaymentService
{
    Task<IReadOnlyList<FeeTypeDto>> GetFeeTypesAsync(bool includeInactive = false);
    Task<FeeTypeDto> SaveFeeTypeAsync(int? id, SaveFeeTypeDto dto);
    Task<IReadOnlyList<FeePaymentDto>> GetForUserAsync(int userId);
    Task<IReadOnlyList<FeePaymentDto>> GetAllAsync(string? status, int? feeTypeId);
    Task<int> AssignAsync(AssignFeeDto dto);
    Task<FeePaymentDto> UpdateAsync(int id, UpdateFeeDto dto);
    Task CancelAsync(int id);
    Task<FeePaymentDto> PayAsync(int id, int userId, bool isAdmin);
    Task<ReceiptDto> GetReceiptAsync(int id, int userId, bool isAdmin);
}
