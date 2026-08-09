using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.DTOs.Fees;

public class FeeTypeDto
{
    public int FeeTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class SaveFeeTypeDto
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(300)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AssignFeeDto
{
    public int? StudentId { get; set; }
    [MaxLength(50)] public string? StudentIndexNumber { get; set; }
    [MaxLength(100)] public string? Faculty { get; set; }
    [Required] public int FeeTypeId { get; set; }
    [Required, MaxLength(100)] public string BillingPeriod { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.01", "99999999")] public decimal Amount { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

public class UpdateFeeDto
{
    [Required, MaxLength(100)] public string BillingPeriod { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.01", "99999999")] public decimal Amount { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

public class FeePaymentDto
{
    public int FeePaymentId { get; set; }
    public int UserId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string IndexNumber { get; set; } = string.Empty;
    public int FeeTypeId { get; set; }
    public string FeeTypeName { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? ReceiptNumber { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class ReceiptDto : FeePaymentDto
{
    public DateTime GeneratedAt { get; set; }
}
